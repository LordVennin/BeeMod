
import asyncio
from dataclasses import dataclass, field
from textwrap import dedent


@dataclass
class Room:
    key: str
    name: str
    description: str
    exits: dict = field(default_factory=dict)


@dataclass
class Player:
    name: str
    room: str
    writer: asyncio.StreamWriter


class MudServer:
    """A tiny two-player-friendly fantasy MUD server for LAN play."""

    def __init__(self, host="0.0.0.0", port=5000):
        # 0.0.0.0 allows connections from the local network.
        self.host = host
        self.port = port
        self.rooms = self._create_world()
        self.players = {}  # writer -> Player

    def _create_world(self):
        """Define a handful of linked rooms for quick exploration."""

        town_square = Room(
            key="square",
            name="Town Square",
            description=(
                "Lanterns flicker against stone walls. Exits lead north to the market "
                "and east toward the guild hall."
            ),
            exits={"north": "market", "east": "hall"},
        )

        market = Room(
            key="market",
            name="Moonlit Market",
            description=(
                "Merchants hawk wares beneath fluttering banners. A path returns south "
                "to the square or west into a shadowy alley."
            ),
            exits={"south": "square", "west": "alley"},
        )

        guild_hall = Room(
            key="hall",
            name="Adventurers' Guild Hall",
            description=(
                "A roaring hearth warms tired travelers. The square lies west; a "
                "staircase climbs up to a quiet balcony."
            ),
            exits={"west": "square", "up": "balcony"},
        )

        balcony = Room(
            key="balcony",
            name="Guild Balcony",
            description=(
                "You overlook the market from here. The only way is back down."
            ),
            exits={"down": "hall"},
        )

        alley = Room(
            key="alley",
            name="Shadowed Alley",
            description=(
                "The alley smells of rain and secrets. Faint light glows to the east; "
                "a narrow passage heads north."),
            exits={"east": "market", "north": "shrine"},
        )

        shrine = Room(
            key="shrine",
            name="Glimmering Shrine",
            description=(
                "Moonlight pours through a broken roof, illuminating an ancient shrine. "
                "The alley lies back south."
            ),
            exits={"south": "alley"},
        )

        rooms = [town_square, market, guild_hall, balcony, alley, shrine]
        return {room.key: room for room in rooms}

    async def start(self):
        server = await asyncio.start_server(
            self.handle_client, self.host, self.port
        )
        print(f"Server running on {self.host}:{self.port}")
        async with server:
            await server.serve_forever()

    async def handle_client(self, reader, writer):
        addr = writer.get_extra_info('peername')
        print(f"New connection from {addr}")

        writer.write(b"Welcome to the LAN MUD!\n")
        writer.write(b"What is your name? ")
        await writer.drain()

        name = (await reader.readline()).decode().strip() or "Anonymous"
        player = Player(name=name, room="square", writer=writer)
        self.players[writer] = player

        await self.show_room(player)
        await self.broadcast_global(f"{player.name} has connected.\n", exclude=player)

        try:
            while True:
                writer.write(b"\n> ")
                await writer.drain()
                line = await reader.readline()
                if not line:
                    break
                command = line.decode().strip()
                await self.process_command(player, command)
        except (ConnectionError, asyncio.CancelledError):
            pass
        finally:
            print(f"{player.name} disconnected")
            del self.players[writer]
            await self.broadcast_global(f"{player.name} has left the realm.\n")
            writer.close()
            await writer.wait_closed()

    async def show_room(self, player):
        room = self.rooms[player.room]
        text = (
            f"\n{room.name}\n"
            f"{room.description}\n"
            f"Exits: {', '.join(room.exits.keys()) or 'none'}\n"
        )
        await self.send_to_player(player, text)

    async def send_to_player(self, player, message):
        player.writer.write(message.encode())
        await player.writer.drain()

    async def broadcast_room(self, room_key, message, exclude=None):
        for p in self.players.values():
            if p.room == room_key and p is not exclude:
                await self.send_to_player(p, message)

    async def broadcast_global(self, message, exclude=None):
        for p in self.players.values():
            if p is not exclude:
                await self.send_to_player(p, message)

    async def process_command(self, player, command):
        if not command:
            return

        parts = command.split(maxsplit=1)
        verb = parts[0].lower()
        arg = parts[1] if len(parts) > 1 else ""

        if verb in ("help", "h", "?"):
            await self.send_to_player(player, self._help_text())

        elif verb in ("look", "l"):
            await self.show_room(player)

        elif verb == "say":
            if not arg:
                await self.send_to_player(player, "Say what?\n")
                return
            await self.broadcast_room(
                player.room,
                f"{player.name} says: {arg}\n",
                exclude=player
            )
            await self.send_to_player(player, f"You say: {arg}\n")

        elif verb == "emote":
            if not arg:
                await self.send_to_player(player, "Emote what?\n")
                return
            await self.broadcast_room(
                player.room,
                f"* {player.name} {arg}\n",
                exclude=player,
            )
            await self.send_to_player(player, f"* You {arg}\n")

        elif verb == "who":
            names = [p.name for p in self.players.values()]
            await self.send_to_player(
                player,
                "Connected players: " + (", ".join(names) if names else "none") + "\n",
            )

        elif verb in ("north", "south", "east", "west", "up", "down", "n", "s", "e", "w", "u", "d"):
            direction = verb[0]  # n/s/e/w/u/d
            dir_map = {
                "n": "north",
                "s": "south",
                "e": "east",
                "w": "west",
                "u": "up",
                "d": "down",
            }
            direction = dir_map.get(direction, direction)

            current_room = self.rooms[player.room]
            if direction in current_room.exits:
                new_room_key = current_room.exits[direction]
                await self.broadcast_room(
                    player.room, f"{player.name} leaves {direction}.\n", exclude=player
                )
                player.room = new_room_key
                await self.broadcast_room(
                    player.room, f"{player.name} enters.\n", exclude=player
                )
                await self.show_room(player)
            else:
                await self.send_to_player(player, "You can't go that way.\n")

        elif verb == "quit":
            await self.send_to_player(player, "Goodbye!\n")
            player.writer.close()

        else:
            await self.send_to_player(player, "Unknown command.\n")

    def _help_text(self):
        return dedent(
            """
            Commands:
              look/l           Show your current room.
              say <message>    Chat with players in your room.
              emote <action>   Perform an action (emote) to the room.
              who              List connected players.
              n,s,e,w,u,d      Move between rooms.
              quit             Leave the server.

            Connect from another machine on your LAN using: telnet <host> 5000
            """
        )

if __name__ == "__main__":
    asyncio.run(MudServer().start())
