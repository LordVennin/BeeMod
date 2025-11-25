
import asyncio
from dataclasses import dataclass, field

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
    def __init__(self, host="0.0.0.0", port=5000):
        self.host = host
        self.port = port
        self.rooms = self._create_world()
        self.players = {}  # writer -> Player

    def _create_world(self):
        # Simple 2-room world
        room1 = Room(
            key="start",
            name="Starting Room",
            description="You are in a small stone room. Exits: east.",
            exits={"east": "hall"}
        )
        room2 = Room(
            key="hall",
            name="Hallway",
            description="A narrow hallway. Exits: west.",
            exits={"west": "start"}
        )
        return {room1.key: room1, room2.key: room2}

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
        player = Player(name=name, room="start", writer=writer)
        self.players[writer] = player

        await self.show_room(player)

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
            writer.close()
            await writer.wait_closed()

    async def show_room(self, player):
        room = self.rooms[player.room]
        text = (
            f"\n{room.name}\n"
            f"{room.description}\n"
        )
        await self.send_to_player(player, text)

    async def send_to_player(self, player, message):
        player.writer.write(message.encode())
        await player.writer.drain()

    async def broadcast_room(self, room_key, message, exclude=None):
        for p in self.players.values():
            if p.room == room_key and p is not exclude:
                await self.send_to_player(p, message)

    async def process_command(self, player, command):
        if not command:
            return

        parts = command.split(maxsplit=1)
        verb = parts[0].lower()
        arg = parts[1] if len(parts) > 1 else ""

        if verb in ("look", "l"):
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

        elif verb in ("north", "south", "east", "west", "n", "s", "e", "w"):
            direction = verb[0]  # n/s/e/w
            dir_map = {"n": "north", "s": "south", "e": "east", "w": "west"}
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

if __name__ == "__main__":
    asyncio.run(MudServer().start())
