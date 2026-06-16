using System.Collections.Concurrent;

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
}