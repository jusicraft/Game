using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Game;

public class Map
{
    
    //private Tuple<Room, Room> Pa
    //private List<Pathway> _pathways;
    private ConcurrentDictionary<(Room room1, Room room2), Pathway> _pathways;
    //private List<Player> _players;
    private ConcurrentDictionary<(string name, Player), Player> _players;
    //private List<Room> _rooms;
    private ConcurrentDictionary<(int x, int y), Room> _rooms;

    public Map()
    {
        _pathways = new ConcurrentDictionary<(Room room1, Room room2), Pathway>();
        _players = new ConcurrentDictionary<(string name, Player), Player>();
        _rooms = new ConcurrentDictionary<(int x, int y), Room>();
    }

    public bool Move(int byX, int byY, Player player)
    {
        Room? currentRoom = GetPlayersCurrentRoom(player);
        Room? nextRoom = FindRoom(player.X + byX, player.Y + byY);
        
        if (currentRoom == null || nextRoom == null || currentRoom == nextRoom)
        {
            return false;
        }
        Pathway? connection = FindConnection(currentRoom, nextRoom);
        if (connection == null)
        {
            return false;
        }

        if (connection.UnlockId > 0 && player.FindKeyById(connection.UnlockId) == null)
        {
            return false;
        }

        var fromPlayers = currentRoom.Players.Where(p => p != player).ToList();
        foreach (var p in fromPlayers)
        {
            p.Writer.WriteLine($"\nHráč {player.Name} odešel do místnosti: {nextRoom.Name}.");
        }

        currentRoom.Players.Remove(player);
        nextRoom.Players.Add(player);

        var toPlayers = nextRoom.Players.Where(p => p != player).ToList();
        foreach (var p in toPlayers)
        {
            p.Writer.WriteLine($"\nHráč {player.Name} vstoupil do místnosti.");
        }

        player.X += byX;
        player.Y += byY;
        return true;
    }

    public Room? FindRoom(int byX, int byY)
    {
        if (_rooms.TryGetValue((byX, byY), out Room? room))
        {
            return room;
        }
        return null;
    }

    public Pathway? FindConnection(Room room1, Room room2)
    {
        if (_pathways.TryGetValue((room1, room2), out Pathway? pathway))
        {
            return pathway;
        }
        return null;
    }

    public Room? GetPlayersCurrentRoom(Player player)
    {
        return FindRoom(player.X, player.Y);
    }
    
    public void AddRoom(Room room)
    {
        _rooms.TryAdd((room.X, room.Y), room);
    }

    public void AddPathway(Pathway pathway)
    {
        //obousmerna cesta
        _pathways.TryAdd((pathway.Room1, pathway.Room2), pathway);
        _pathways.TryAdd((pathway.Room2, pathway.Room1), pathway);
    }

    public List<Pathway> GetPathwaysFromRoom(Room room)
    {
        return _pathways.Keys
            .Where(k => k.room1 == room)
            .Select(k => _pathways[k])
            .OrderBy(p => (p.Room1 == room ? p.Room2 : p.Room1).Name)
            .ToList();
    }

    public List<string> GetAllNpcNames()
    {
        return _rooms.Values
            .SelectMany(r => r.Npcs)
            .Select(n => n.Name)
            .Distinct()
            .ToList();
    }

    public List<Room> GetAllRooms()
    {
        return _rooms.Values.ToList();
    }

    public string GetKeyName(int unlockId, Player player)
    {
        var key = player.FindKeyById(unlockId);
        if (key != null) return key.Name;

        foreach (var room in _rooms.Values)
        {
            foreach (var item in room.Items)
            {
                if (item is Key k && k.Id == unlockId)
                {
                    return k.Name;
                }
            }
        }

        return "klíč";
    }

    public void AddPlayer(Player player)
    {
        _players.TryAdd((player.Name.ToLowerInvariant(), player), player);
    }

    public void RemovePlayer(Player player)
    {
        _players.TryRemove((player.Name.ToLowerInvariant(), player), out _);
    }

    public List<Player> GetAllPlayers()
    {
        return _players.Values.ToList();
    }
}