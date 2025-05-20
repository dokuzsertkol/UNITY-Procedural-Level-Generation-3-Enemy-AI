using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class levelGenerator : MonoBehaviour
{
    public enum CellType
    {
        None, Room, MainRoom, HallwayUnread, Hallway, HallwayBorderNew, HallwayBorderUnread, HallwayBorder, HallwayFieldUnread, HallwayField, HallwayCornerUnread, HallwayCorner
    }
    public enum Way
    {
        top, bottom, left, right
    }
    public enum Materials
    {
        floor, celling, wall
    }
    public enum RoomType
    {
        Empty, Study, Saloon, Bed, Storage
    }
    private class Block
    {
        public GameObject prefab;
        public Vector3 pos, size;
        public Quaternion rotation = Quaternion.identity;
        public Material material;
        public Block(GameObject prefab, Vector3 pos)
        {
            this.prefab = prefab;
            this.pos = pos;
        }
        public Block(GameObject prefab, Vector3 pos, Vector3 size, Material material)
        {
            this.prefab = prefab;
            this.pos = pos;
            this.size = size;
            this.material = material;
        }
        public Block(GameObject prefab, Vector3 pos, Vector3 size, Material material, Quaternion rotation)
        {
            this.prefab = prefab;
            this.pos = pos;
            this.size = size;
            this.material = material;
            this.rotation = rotation;
        }
    }
    private class Room
    {
        public int id;
        public RoomType roomType;
        public Vector3Int pos, size; // pos.y = 0

        private Vector3Int hallwaySize;
        private bool left = false, right = false, top = false, bottom = false, celling;
        private GameObject prefab;
        private Material[] materials;
        public Room()
        {
            id = -1;
        }
        public Room(int id, Vector3Int pos, Vector3Int size, Vector3Int hallwaySize, GameObject prefab, Material[] materials, bool celling, RoomType roomType)
        {
            this.id = id;
            this.pos = pos;
            this.size = size;
            this.prefab = prefab;
            this.hallwaySize = hallwaySize;
            this.materials = materials;
            this.celling = celling;
            this.roomType = roomType;
            /*
            Random rand = new(DateTime.Now.Millisecond);
            var values = Enum.GetValues(typeof(RoomType));
            this.roomType = (RoomType) values.GetValue(rand.Next(values.Length));*/
            Debug.Log(id + " " + roomType);
        }
        public void SetEntrance(Way way)
        {
            switch (way)
            {
                case Way.left:
                    this.left = true; break;
                case Way.right:
                    this.right = true; break;
                case Way.top:
                    this.top = true; break;
                case Way.bottom:
                    this.bottom = true; break;
            }
        }
        public List<Block> GetBlocks()
        {
            List<Block> blocks = new();
            // zemin ve tavan
            Block block = new(prefab, new Vector3(pos.x, pos.y, pos.z), new Vector3(size.x, 1, size.z), materials[(int)Materials.floor]);
            blocks.Add(block);
            if (celling)
            {
                block = new(prefab, new Vector3(pos.x, pos.y + size.y + 1, pos.z), new Vector3(size.x, 1, size.z), materials[(int)Materials.celling]);
                blocks.Add(block);
            }
            // duvarlar
            foreach (var k in Enum.GetValues(typeof(Way)))
            {
                Vector3 blockPos = new(pos.x, pos.y + (size.y + 1) / 2, pos.z), blockSize = new(size.x, size.y, size.z);
                switch ((Way)k)
                {
                    case Way.top:
                        blockPos.z = pos.z + ((size.z - 1) / 2.0f + 0.25f);
                        blockSize.z = 0.5f;
                        break;
                    case Way.bottom:
                        blockPos.z = pos.z - ((size.z - 1) / 2.0f + 0.25f);
                        blockSize.z = 0.5f;
                        break;
                    case Way.left:
                        blockPos.x = pos.x - ((size.x - 1) / 2.0f + 0.25f);
                        blockSize.x = 0.5f;
                        blockSize.z -= 1;
                        break;
                    case Way.right:
                        blockPos.x = pos.x + ((size.x - 1) / 2.0f + 0.25f);
                        blockSize.x = 0.5f;
                        blockSize.z -= 1;
                        break;
                }
                if ((Way)k == Way.left && left || (Way)k == Way.right && right || (Way)k == Way.top && top || (Way)k == Way.bottom && bottom)
                {
                    float entranceWideX = hallwaySize.x * 2 - 1;
                    float entranceWideZ = hallwaySize.z * 2 - 1;
                    Vector3 blockPos1 = blockPos, blockPos2 = blockPos;
                    switch ((Way)k)
                    {
                        case Way.top:
                            blockSize.x = (size.x - entranceWideX) / 2;
                            blockPos1.x -= entranceWideX / 2 + blockSize.x / 2;
                            blockPos2.x += entranceWideX / 2 + blockSize.x / 2;
                            break;
                        case Way.bottom:
                            blockSize.x = (size.x - entranceWideX) / 2;
                            blockPos1.x -= entranceWideX / 2 + blockSize.x / 2;
                            blockPos2.x += entranceWideX / 2 + blockSize.x / 2;
                            break;
                        case Way.left:
                            blockSize.z = (size.z - entranceWideZ - 1) / 2;
                            blockPos1.z -= entranceWideZ / 2 + blockSize.z / 2;
                            blockPos2.z += entranceWideZ / 2 + blockSize.z / 2;
                            break;
                        case Way.right:
                            blockSize.z = (size.z - entranceWideZ - 1) / 2;
                            blockPos1.z -= entranceWideZ / 2 + blockSize.z / 2;
                            blockPos2.z += entranceWideZ / 2 + blockSize.z / 2;
                            break;
                    }
                    block = new(prefab, blockPos1, blockSize, materials[(int)Materials.wall]);
                    blocks.Add(block);
                    block = new(prefab, blockPos2, blockSize, materials[(int)Materials.wall]);
                    blocks.Add(block);
                }
                else
                {
                    block = new(prefab, blockPos, blockSize, materials[(int)Materials.wall]);
                    blocks.Add(block);
                }
            }
            return blocks;
        }
    }

    public bool celling; // tavan olsun mu olmasýn mý
    public int roomCount, collectableCount;
    public Vector3Int gridSize, mainRoomSize, roomMinSize, roomMaxSize, hallwaySize;
    public GameObject cubePrefab, tableChair, wandererPrefab, sleeperPrefab, screamerPrefab, collectablePrefab;
    public Material roomWallMaterial, roomFloorMaterial, roomCellingMaterial,
        hallwayWallMaterial, hallwayFloorMaterial, hallwayCellingMaterial;

    public static CellType[][] mapGrid;

    private int distanceBetweenRooms;
    private Material[] roomMaterials = new Material[Enum.GetNames(typeof(Materials)).Length];
    private GameObject mapObject;
    private Random random;
    private NavMeshSurface navMeshSurface;
    private List<Block> blocks = new();
    private List<Room> rooms = new();
    private player player;


    // sonra doorscript içine alýnacak
    private Healthbar healthbar;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<player>();
        healthbar = GameObject.FindWithTag("healthbar").GetComponent<Healthbar>();

        navMeshSurface = GetComponent<NavMeshSurface>();

        roomMaterials[(int)Materials.celling] = roomCellingMaterial;
        roomMaterials[(int)Materials.floor] = roomFloorMaterial;
        roomMaterials[(int)Materials.wall] = roomWallMaterial;

        distanceBetweenRooms = Math.Max(hallwaySize.x * 2 + 1, hallwaySize.z * 2 + 1);

        mapGrid = new CellType[gridSize.x][];
        for (int i = 0; i < gridSize.x; i++)
        {
            mapGrid[i] = new CellType[gridSize.z];
            for (int j = 0; j < gridSize.z; j++)
            {
                mapGrid[i][j] = CellType.None;
            }
        }
    }
    public void Generate()
    {
        healthbar.StopTimer(); // sonra doorscripte alýnacak


        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.z; j++)
            {
                mapGrid[i][j] = CellType.None;
            }
        }
        blocks.Clear();
        rooms.Clear();
        GameObject.Destroy(mapObject);

        random = new Random(DateTime.Now.Millisecond);

        navMeshSurface.RemoveData();
        navMeshSurface.BuildNavMesh();
        EnterMainRoomToGrid();
        CreateRooms();
        CreateHallways();
        HallwayEnlargement();
        FillRooms();
        Build();
        // spawn tables etc
        SpawnPrefabs();

        StartCoroutine(CreateNavMesh());
    }
    private void EnterMainRoomToGrid()
    {
        Vector3Int roomPos, roomBottomLeft, roomTopRight;
        roomPos = new Vector3Int(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);
        roomBottomLeft = roomPos - (mainRoomSize - Vector3Int.one);
        roomTopRight = roomPos + (mainRoomSize - Vector3Int.one);

        for (int j = roomBottomLeft.x; j <= roomTopRight.x; j++) for (int k = roomBottomLeft.z; k <= roomTopRight.z; k++) mapGrid[j][k] = CellType.MainRoom;
    }
    private void CreateRooms()
    {
        Vector3Int roomPos = new(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2),
            roomSize = new(13, roomMinSize.y, 5), roomHalfSize = new(6, roomMinSize.y, 2), 
            roomBottomLeft, roomTopRight;
        int maxDeneme = 100, id = 3;
        bool ok;

        //main odanýn baþýna ve sonuna oda koy
        Room room = new(1, roomPos + new Vector3Int(0, 0, mainRoomSize.z + roomHalfSize.z), roomSize, hallwaySize, cubePrefab, roomMaterials, celling, RoomType.Empty);
        room.SetEntrance(Way.bottom);
        rooms.Add(room);
        room = new(2, roomPos - new Vector3Int(0, 0, mainRoomSize.z + roomHalfSize.z), roomSize, hallwaySize, cubePrefab, roomMaterials, celling, RoomType.Empty);
        room.SetEntrance(Way.top);
        rooms.Add(room);
        for (int i = rooms[1].pos.x - roomHalfSize.x; i <= rooms[1].pos.x + roomHalfSize.x; i++) for (int j = rooms[0].pos.z - roomHalfSize.z; j <= rooms[0].pos.z + roomHalfSize.z; j++) mapGrid[i][j] = CellType.Room;
        for (int i = rooms[1].pos.x - roomHalfSize.x; i <= rooms[1].pos.x + roomHalfSize.x; i++) for (int j = rooms[1].pos.z - roomHalfSize.z; j <= rooms[1].pos.z + roomHalfSize.z; j++) mapGrid[i][j] = CellType.Room;
        
        // diðer tüm odalarý oluþtur
        for (int i = 0; i < roomCount; i++)
        {
            if (maxDeneme == 0) break;
            maxDeneme--;
            ok = true;

            roomPos = new Vector3Int(random.Next(roomMinSize.x / 2, (gridSize.x - roomMinSize.x) / 2), 0, random.Next(roomMinSize.z / 2, (gridSize.z - roomMinSize.z) / 2));
            roomPos *= 2;
            roomHalfSize = new Vector3Int(random.Next(roomMinSize.x, roomMaxSize.x + 1), 0, random.Next(roomMinSize.z, roomMaxSize.z + 1));
            roomSize = roomHalfSize * 2 - Vector3Int.one;
            roomSize.y = random.Next(roomMinSize.y, roomMaxSize.y + 1);
            // grid'in içinde mi
            roomBottomLeft = roomPos - roomHalfSize + Vector3Int.one;
            roomTopRight = roomPos + roomHalfSize - Vector3Int.one;

            if (roomBottomLeft.x < 0 || roomBottomLeft.z < 0 || roomTopRight.x >= gridSize.x || roomTopRight.z >= gridSize.z)
            {
                i--;
                continue;
            }
            // herhangi bir odayla çakýþýyor mu
            for (int j = roomBottomLeft.x - distanceBetweenRooms; ok && j <= roomTopRight.x + distanceBetweenRooms; j++)
            {
                for (int k = roomBottomLeft.z - distanceBetweenRooms; k <= roomTopRight.z + distanceBetweenRooms; k++)
                {
                    if (j < 0 || k < 0 || j >= gridSize.x || k >= gridSize.z) continue;
                    if (mapGrid[j][k] == CellType.Room || mapGrid[j][k] == CellType.MainRoom)
                    {
                        i--;
                        ok = false;
                        break;
                    }
                }
                if (!ok) break;
            }
            // ikisi de hayýrsa tamam
            if (ok)
            {
                for (int j = roomBottomLeft.x; j <= roomTopRight.x; j++) for (int k = roomBottomLeft.z; k <= roomTopRight.z; k++) mapGrid[j][k] = CellType.Room;
                var values = Enum.GetValues(typeof(RoomType));
                RoomType roomType = (RoomType)values.GetValue(random.Next(0, values.Length));
                room = new(id, roomPos, roomSize, hallwaySize, cubePrefab, roomMaterials, celling, roomType);
                id++;
                rooms.Add(room);
            }
        }
    }
    private void CreateHallways()
    {
        if (rooms.Count <= 1) return;
        List<Room[]> roomCouples = new();

        // odalarý baðla
        foreach (var room1 in rooms)
        {
            if (room1 == rooms[^1]) break;
            Room[] couple = { room1, FindClosestPossibleRoom(room1, rooms, roomCouples) };
            if (couple[0].id != couple[1].id && couple[0].id + couple[1].id == 3)
            {
                roomCouples.Add(couple);
            }
        }

        // birbirine baðlanan odalarý tespit et
        List<List<Room>> seperateRoomCouples = new();
        List<Room> remainingNodes = new(rooms), connectedRooms = new(), randomRoom = new(), newRandomRoom = new();
        bool finished;
        while (remainingNodes.Count > 0)
        {
            randomRoom.Add(remainingNodes[0]);
            connectedRooms.Clear();
            connectedRooms.Add(remainingNodes[0]);
            remainingNodes.Remove(remainingNodes[0]);

            finished = false;
            while (!finished)
            {
                finished = true;
                foreach (var random in randomRoom)
                {
                    foreach (var couple in roomCouples)
                    {
                        if (couple[0] == random && !connectedRooms.Contains(couple[1]))
                        {
                            connectedRooms.Add(couple[1]);
                            newRandomRoom.Add(couple[1]);
                            remainingNodes.Remove(couple[1]);
                            finished = false;
                        }
                        if (couple[1] == random && !connectedRooms.Contains(couple[0]))
                        {
                            connectedRooms.Add(couple[0]);
                            newRandomRoom.Add(couple[0]);
                            remainingNodes.Remove(couple[0]);
                            finished = false;
                        }
                    }
                }
                randomRoom = new(newRandomRoom);
                newRandomRoom.Clear();
            }
            randomRoom.Clear();
            seperateRoomCouples.Add(new List<Room>(connectedRooms));
        }
        EnterHallwaysToGrid(roomCouples);
        roomCouples.Clear();

        // odalar topluluklarýný birbirine baðla
        List<int[]> srcss = new();
        foreach (var src1 in seperateRoomCouples)
        {
            foreach (var src2 in seperateRoomCouples)
            {
                if (src1[0].id == src2[0].id) continue;
                bool ok = true;
                foreach (var srcs in srcss) if (srcs[0] == src1[0].id && srcs[1] == src2[0].id || srcs[1] == src1[0].id && srcs[0] == src2[0].id) { ok = false; break; }
                if (!ok) continue;
   
                int distance, minDistance = int.MaxValue;
                Room finalRoomInRc1 = new(), finalRoomInRc2 = new();
                foreach (var r in src1)
                {
                    Room closestRoom = FindClosestPossibleRoom(r, src2, roomCouples);
                    if (r.id == closestRoom.id) continue;
                    distance = Math.Abs(r.pos.x - closestRoom.pos.x) + Math.Abs(r.pos.y - closestRoom.pos.y) + Math.Abs(r.pos.z - closestRoom.pos.z);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        finalRoomInRc1 = r;
                        finalRoomInRc2 = closestRoom;
                    }
                }
                if (finalRoomInRc1.id != -1 && finalRoomInRc2.id != -1)
                {
                    Room[] couple = { finalRoomInRc1, finalRoomInRc2 };
                    roomCouples.Add(couple);

                    int[] couple_ = { src1[0].id, src2[0].id };
                    srcss.Add(couple_);
                }
            }
        }
        EnterHallwaysToGrid(roomCouples);
    }
    private Room FindClosestPossibleRoom(Room room1, List<Room> nodes, List<Room[]> roomCouples)
    {
        int distance, minDistance;
        Room closestRoom = new();
        List<Room> remainingRooms = new(nodes);
        remainingRooms.Remove(room1);
        foreach (var coup in roomCouples)
        {
            if (coup[0].id == room1.id) remainingRooms.Remove(coup[1]);
            else if (coup[1].id == room1.id) remainingRooms.Remove(coup[0]);
        }
        bool collapse = true, notCollapsedAtLeastOnce = false;

        int maxtry = 1000;
        while (collapse)
        {
            if (maxtry == 0) { Debug.LogError("elma" + room1.id);  break; }
            else maxtry--;
            minDistance = int.MaxValue;
            if (remainingRooms.Count == 0) break;
            foreach (var room2 in remainingRooms)
            {
                //if (room1.id + room2.id == 3) { Debug.Log("yakalaadým"); continue; }// 1 ve 2 id'li odalarý birbirine baðlama (main odanýn baþýndaki odalar)
                distance = Math.Abs(room1.pos.x - room2.pos.x) + Math.Abs(room1.pos.y - room2.pos.y) + Math.Abs(room1.pos.z - room2.pos.z);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRoom = room2;
                }
            }
            if (room1.id + closestRoom.id == 3) collapse = true;
            else collapse = CheckPath(room1.pos, closestRoom.pos);
            if (collapse) remainingRooms.Remove(closestRoom);
            else notCollapsedAtLeastOnce = true;
        }
        return notCollapsedAtLeastOnce ? closestRoom : room1;
    }
    private bool CheckPath(Vector3Int pos1, Vector3Int pos2)
    {
        bool collapse = false, firstX;
        Vector2Int topLeft, bottomRight, offset;

        if (Math.Abs(pos1.x - pos2.x) < Math.Abs(pos1.z - pos2.z))
        {
            offset = new(hallwaySize.x, 0);
            firstX = false;
        }
        else
        {
            offset = new(0, hallwaySize.z);
            firstX = true;
        }

        Vector3Int[] result = FindPathStartAndFinish(pos1, pos2, firstX);
        pos1 = result[0];
        pos2 = result[1];

        if (firstX && Mathf.Abs(pos1.x - pos2.x) < hallwaySize.x * 2) return true;
        else if (!firstX && Mathf.Abs(pos1.z - pos2.z) < hallwaySize.z * 2) return true;

        if (pos1.x < pos2.x)
        {
            if (pos1.z < pos2.z)
            {
                topLeft = new(pos2.x + 1, pos2.z + 1);
                bottomRight = new(pos1.x, pos1.z);
            }
            else
            {
                topLeft = new(pos2.x + 1, pos1.z + 1);
                bottomRight = new(pos1.x, pos2.z);
            }
        }
        else
        {
            if (pos1.z < pos2.z)
            {
                topLeft = new(pos1.x + 1, pos2.z + 1);
                bottomRight = new(pos2.x, pos1.z);
            }
            else
            {
                topLeft = new(pos1.x + 1, pos1.z + 1);
                bottomRight = new(pos2.x, pos2.z);
            }
        }
        topLeft += offset;
        bottomRight -= offset;

        for (int k = bottomRight[0]; k < topLeft[0]; k++)
        {
            for (int j = bottomRight[1]; j < topLeft[1]; j++)
            {
                if ((k == pos1.x && j == pos1.z) || (k == pos2.x && j == pos2.z)) continue;

                if (mapGrid[k][j] == CellType.Room || mapGrid[k][j] == CellType.MainRoom) // out of bound yedi kodda baþka hata varken
                {
                    collapse = true;
                    break;
                }
            }
            if (collapse) break;
        }
        return collapse;
    }
    private Vector3Int[] FindPathStartAndFinish(Vector3Int pos1, Vector3Int pos2, bool firstX)
    {
        Vector3Int[] result = { pos1, pos2 };
        if (pos1.x >= gridSize.x || pos1.z >= gridSize.z || pos2.x >= gridSize.x || pos2.z >= gridSize.z ||
            pos1.x < 0 || pos1.z < 0 || pos2.x < 0 || pos2.z < 0) return result;

        short[] way = new short[2];
        if (pos1.x > pos2.x) way[0] = -1;
        else way[0] = 1;
        if (pos1.z > pos2.z) way[1] = -1;
        else way[1] = 1;
        while (mapGrid[pos1.x][pos1.z] == CellType.Room)
        {
            if (firstX)
            {
                pos1 += new Vector3Int(way[0], 0, 0);
                if (pos1.x == pos2.x) break;
            }
            else
            {
                pos1 += new Vector3Int(0, 0, way[1]);
                if (pos1.z == pos2.z) break;
            }
        }
        if (pos1.x >= gridSize.x || pos1.z >= gridSize.z || pos2.x >= gridSize.x || pos2.z >= gridSize.z) return result;
        while (mapGrid[pos2.x][pos2.z] == CellType.Room || mapGrid[pos2.x][pos2.z] == CellType.MainRoom)
        {
            if (firstX)
            {
                pos2 -= new Vector3Int(way[0], 0, 0);
                if (pos1.x == pos2.x) break;
            }
            else
            {
                pos2 -= new Vector3Int(0, 0, way[1]);
                if (pos1.z == pos2.z) break;
            }
        }

        result[0] = pos1;
        result[1] = pos2;
        return result;
    }
    private void EnterHallwaysToGrid(List<Room[]> roomCouple)
    {
        if (roomCouple.Count < 1) return;
        Vector2Int topLeft, bottomRight, offset;
        Vector3Int pos1, pos2, ogpos1;
        Vector3Int[] result;
        bool firstX;
        int changeCount, halfDistance;

        foreach (var couple in roomCouple)
        {
            pos1 = couple[0].pos; // sender
            pos2 = couple[1].pos; // receiver
            ogpos1 = pos1;
            changeCount = 0;

            if (Math.Abs(pos1.x - pos2.x) < Math.Abs(pos1.z - pos2.z))
            {
                offset = new(hallwaySize.x, 0);
                firstX = false;
            }
            else
            {
                offset = new(0, hallwaySize.z);
                firstX = true;
            }

            // odanýn ortasý yerine sonundan baþlasýn diye
            result = FindPathStartAndFinish(pos1, pos2, firstX);
            pos1 = result[0];
            pos2 = result[1];

            if (pos1.x < pos2.x)
            {
                if (pos1.z < pos2.z)
                {
                    topLeft = new(pos2.x, pos2.z);
                    bottomRight = new(pos1.x, pos1.z);
                }
                else
                {
                    topLeft = new(pos2.x, pos1.z);
                    bottomRight = new(pos1.x, pos2.z);
                }
            }
            else
            {
                if (pos1.z < pos2.z)
                {
                    topLeft = new(pos1.x, pos2.z);
                    bottomRight = new(pos2.x, pos1.z);
                }
                else
                {
                    topLeft = new(pos1.x, pos1.z);
                    bottomRight = new(pos2.x, pos2.z);
                }
            }
            topLeft += offset;
            bottomRight -= offset;
            for (int i = bottomRight.x; i <= topLeft.x; i++) for (int j = bottomRight.y; j <= topLeft.y; j++) if (mapGrid[i][j] == CellType.None) mapGrid[i][j] = CellType.HallwayFieldUnread;

            if (firstX)
            {
                halfDistance = (pos1.x - pos2.x) / 2;
                if(pos1.x < pos2.x)
                {
                    couple[0].SetEntrance(Way.right);
                    couple[1].SetEntrance(Way.left);
                }
                else
                {
                    couple[0].SetEntrance(Way.left);
                    couple[1].SetEntrance(Way.right);
                }
            }
            else
            {
                halfDistance = (pos1.z - pos2.z) / 2;
                if (pos1.z < pos2.z)
                {
                    couple[0].SetEntrance(Way.top);
                    couple[1].SetEntrance(Way.bottom);
                }
                else
                {
                    couple[0].SetEntrance(Way.bottom);
                    couple[1].SetEntrance(Way.top);
                }
            }

            while (pos1 != pos2)
            {
                mapGrid[pos1.x][pos1.z] = CellType.HallwayBorderUnread;

                if ((changeCount == 0 && (firstX ? pos1.x == pos2.x + halfDistance : pos1.z == pos2.z + halfDistance)) ||
                    changeCount == 1 && (firstX ? (pos1.z == pos2.z) : (pos1.x == pos2.x)))
                {
                    firstX = !firstX;
                    changeCount++;
                }

                if (firstX)
                {
                    if (pos1.x > pos2.x) pos1 -= new Vector3Int(1, 0, 0);
                    else if (pos1.x < pos2.x) pos1 += new Vector3Int(1, 0, 0);
                    else firstX = false;
                }
                else
                {
                    if (pos1.z > pos2.z) pos1 -= new Vector3Int(0, 0, 1);
                    else if (pos1.z < pos2.z) pos1 += new Vector3Int(0, 0, 1);
                    else firstX = true;
                }
            }
            mapGrid[pos2.x][pos2.z] = CellType.HallwayBorderUnread;
        }
    }
    private void HallwayEnlargement()
    {
        for (int k = 0; k < hallwaySize.x; k++)
        {
            for (int i = 0; i < gridSize.x; i++) for (int j = 0; j < gridSize.z; j++) if (mapGrid[i][j] == CellType.HallwayBorderUnread)
                    {
                        mapGrid[i][j] = CellType.HallwayUnread;
                        if (i + 1 < gridSize.x && mapGrid[i + 1][j] == CellType.HallwayFieldUnread) mapGrid[i + 1][j] = CellType.HallwayBorderNew;
                        if (i > 0 && mapGrid[i - 1][j] == CellType.HallwayFieldUnread) mapGrid[i - 1][j] = CellType.HallwayBorderNew;
                        if (j + 1 < gridSize.z && mapGrid[i][j + 1] == CellType.HallwayFieldUnread) mapGrid[i][j + 1] = CellType.HallwayBorderNew;
                        if (j > 0 && mapGrid[i][j - 1] == CellType.HallwayFieldUnread) mapGrid[i][j - 1] = CellType.HallwayBorderNew;
                    }
            for (int i = 0; i < gridSize.x; i++) for (int j = 0; j < gridSize.z; j++) if (mapGrid[i][j] == CellType.HallwayBorderNew) mapGrid[i][j] = CellType.HallwayBorderUnread;
        }
        DeleteHallwayCorners();
    }
    private void DeleteHallwayCorners()
    {
        for (int i = 1; i < gridSize.x - 1; i++)
        {
            for (int j = 1; j < gridSize.z - 1; j++)
            {
                if (mapGrid[i][j] == CellType.HallwayBorderUnread)
                {
                    bool leftWall = mapGrid[i - 1][j] == CellType.HallwayBorderUnread || mapGrid[i - 1][j] == CellType.HallwayCornerUnread || mapGrid[i - 1][j] == CellType.Room;
                    bool rightWall = mapGrid[i + 1][j] == CellType.HallwayBorderUnread || mapGrid[i + 1][j] == CellType.HallwayCornerUnread || mapGrid[i + 1][j] == CellType.Room;
                    bool topWall = mapGrid[i][j + 1] == CellType.HallwayBorderUnread || mapGrid[i][j + 1] == CellType.HallwayCornerUnread || mapGrid[i][j + 1] == CellType.Room;
                    bool bottomWall = mapGrid[i][j - 1] == CellType.HallwayBorderUnread || mapGrid[i][j - 1] == CellType.HallwayCornerUnread || mapGrid[i][j - 1] == CellType.Room;

                    bool leftHallway = mapGrid[i - 1][j] == CellType.HallwayUnread;
                    bool rightHallway = mapGrid[i + 1][j] == CellType.HallwayUnread;
                    bool topHallway = mapGrid[i][j + 1] == CellType.HallwayUnread;
                    bool bottomHallway = mapGrid[i][j - 1] == CellType.HallwayUnread;

                    // iç duvar
                    if ((leftWall ^ rightWall) && (topWall ^ bottomWall) && 
                        (!leftWall || rightHallway) && (!rightWall || leftHallway) &&
                        (!topWall || bottomHallway) && (!bottomWall || topHallway))
                    {
                        if (mapGrid[i - 1][j] == CellType.HallwayCornerUnread)
                        {
                            mapGrid[i][j] = CellType.HallwayBorderUnread;
                            mapGrid[i - 1][j] = CellType.HallwayBorderUnread;
                        }
                        else if(mapGrid[i][j - 1] == CellType.HallwayCornerUnread)
                        {
                            mapGrid[i][j] = CellType.HallwayBorderUnread;
                            mapGrid[i][j - 1] = CellType.HallwayBorderUnread;
                        }
                        else mapGrid[i][j] = CellType.HallwayCornerUnread;

                    }
                }
            }
        }
    }
    private void FillRooms()
    {
        foreach (var room in rooms)
        {
            switch (room.roomType)
            {
                case RoomType.Study:

                    Block block = new(tableChair, room.pos);
                    blocks.Add(block);
                    break;
                case RoomType.Bed:
                    break;
                case RoomType.Saloon:
                    break;
                case RoomType.Storage:
                    break;
            }
        }
    }
    private void Build()
    {
        mapObject = new() { name = "map", tag = "mapObject" };

        // odalar
        foreach (var r in rooms) foreach(var b in r.GetBlocks()) blocks.Add(b);

        // koridorlar
        // iç köþe duvarlar
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.z; j++)
            {
                if (i > 0 && i < gridSize.x && j > 0 && j < gridSize.z && mapGrid[i][j] == CellType.HallwayCornerUnread)
                {
                    mapGrid[i][j] = CellType.HallwayCorner;
                    bool leftWall = mapGrid[i - 1][j] == CellType.HallwayBorderUnread || mapGrid[i - 1][j] == CellType.HallwayCornerUnread || mapGrid[i - 1][j] == CellType.Room || mapGrid[i - 1][j] == CellType.HallwayCorner;
                    bool rightWall = mapGrid[i + 1][j] == CellType.HallwayBorderUnread || mapGrid[i + 1][j] == CellType.HallwayCornerUnread || mapGrid[i + 1][j] == CellType.Room || mapGrid[i + 1][j] == CellType.HallwayCorner;
                    bool topWall = mapGrid[i][j + 1] == CellType.HallwayBorderUnread || mapGrid[i][j + 1] == CellType.HallwayCornerUnread || mapGrid[i][j + 1] == CellType.Room || mapGrid[i][j + 1] == CellType.HallwayCorner;
                    bool bottomWall = mapGrid[i][j - 1] == CellType.HallwayBorderUnread || mapGrid[i][j - 1] == CellType.HallwayCornerUnread || mapGrid[i][j - 1] == CellType.Room || mapGrid[i][j - 1] == CellType.HallwayCorner;

                    int blockEndX = i, blockEndZ = j;
                    /*if (mapGrid[i + 1][j + 1] == CellType.HallwayCornerUnread)
                    {
                        for (int ii = 1; mapGrid[i + ii][j + ii] == CellType.HallwayCornerUnread; ii++)
                        {
                            blockEndX += ii;
                            blockEndZ += ii;
                            mapGrid[i + ii][j + ii] = CellType.HallwayBorder;
                        }
                    }
                    else if (mapGrid[i + 1][j - 1] == CellType.HallwayCornerUnread)
                    {
                        for (int ii = 1; mapGrid[i + ii][j - ii] == CellType.HallwayCornerUnread; ii++)
                        {
                            blockEndX += ii;
                            blockEndZ -= ii;
                        }
                    }*/

                    Vector3 pos = new((i + blockEndX) / 2.0f, hallwaySize.y / 2 + 1, (j + blockEndZ) / 2.0f);
                    Vector3 size = new((blockEndX - i + 1) * Mathf.Sqrt(2), hallwaySize.y, 0.5f);
                    Vector3 rotation = Vector3.zero;
                    if (rightWall && topWall)
                    {
                        pos.x += Mathf.Sqrt(2) / 8;
                        pos.z += Mathf.Sqrt(2) / 8;
                        rotation.y = 45;
                    }
                    else if (leftWall && bottomWall)
                    {
                        pos.x -= Mathf.Sqrt(2) / 8;
                        pos.z -= Mathf.Sqrt(2) / 8;
                        rotation.y = 45;
                    }
                    else if (rightWall && bottomWall)
                    {
                        pos.x += Mathf.Sqrt(2) / 8;
                        pos.z -= Mathf.Sqrt(2) / 8;
                        rotation.y = -45;
                    }
                    else if (leftWall && topWall)
                    {
                        pos.x -= Mathf.Sqrt(2) / 8;
                        pos.z += Mathf.Sqrt(2) / 8;
                        rotation.y = -45;

                    }
                    Block block = new(cubePrefab, pos, size, hallwayWallMaterial, Quaternion.Euler(rotation));
                    blocks.Add(block);
                }
            }
        }
        for (int i = 0; i < gridSize.x; i++) for (int j = 0; j < gridSize.z; j++) if (mapGrid[i][j] == CellType.HallwayCorner) mapGrid[i][j] = CellType.HallwayFieldUnread;
        // dýþ duvarlar
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.z; j++)
            {
                if (i > 0 && i < gridSize.x && j > 0 && j < gridSize.z && mapGrid[i][j] == CellType.HallwayUnread)
                {
                    bool leftWall = mapGrid[i - 1][j] == CellType.HallwayBorderUnread;
                    bool rightWall = mapGrid[i + 1][j] == CellType.HallwayBorderUnread;
                    bool topWall = mapGrid[i][j + 1] == CellType.HallwayBorderUnread;
                    bool bottomWall = mapGrid[i][j - 1] == CellType.HallwayBorderUnread;
                    
                    bool leftHallway = mapGrid[i - 1][j] == CellType.HallwayUnread;
                    bool rightHallway = mapGrid[i + 1][j] == CellType.HallwayUnread;
                    bool topHallway = mapGrid[i][j + 1] == CellType.HallwayUnread;
                    bool bottomHallway = mapGrid[i][j - 1] == CellType.HallwayUnread;

                    if (!((leftWall ^ rightWall) && (topWall ^ bottomWall) &&
                        (!leftWall || rightHallway) && (!rightWall || leftHallway) &&
                        (!topWall || bottomHallway) && (!bottomWall || topHallway))) continue;

                    int blockEndX = i, blockEndZ = j;
                    /*if (mapGrid[i + 1][j + 1] == CellType.HallwayCornerUnread)
                    {
                        for (int ii = 1; mapGrid[i + ii][j + ii] == CellType.HallwayCornerUnread; ii++)
                        {
                            blockEndX += ii;
                            blockEndZ += ii;
                            mapGrid[i + ii][j + ii] = CellType.HallwayBorder;
                        }
                    }
                    else if (mapGrid[i + 1][j - 1] == CellType.HallwayCornerUnread)
                    {
                        for (int ii = 1; mapGrid[i + ii][j - ii] == CellType.HallwayCornerUnread; ii++)
                        {
                            blockEndX += ii;
                            blockEndZ -= ii;
                        }
                    }*/

                    Vector3 pos = new((i + blockEndX) / 2.0f, hallwaySize.y / 2 + 1, (j + blockEndZ) / 2.0f);
                    Vector3 size = new((blockEndX - i + 1) * Mathf.Sqrt(2), hallwaySize.y, 0.5f);
                    Vector3 rotation = Vector3.zero;
                    if (rightWall && topWall)
                    {
                        pos.x += Mathf.Sqrt(2) / 8;
                        pos.z += Mathf.Sqrt(2) / 8;
                        rotation.y = 45;
                    }
                    else if (leftWall && bottomWall)
                    {
                        pos.x -= Mathf.Sqrt(2) / 8;
                        pos.z -= Mathf.Sqrt(2) / 8;
                        rotation.y = 45;
                    }
                    else if (rightWall && bottomWall)
                    {
                        pos.x += Mathf.Sqrt(2) / 8;
                        pos.z -= Mathf.Sqrt(2) / 8;
                        rotation.y = -45;
                    }
                    else if (leftWall && topWall)
                    {
                        pos.x -= Mathf.Sqrt(2) / 8;
                        pos.z += Mathf.Sqrt(2) / 8;
                        rotation.y = -45;

                    }
                    Block block = new(cubePrefab, pos, size, hallwayWallMaterial, Quaternion.Euler(rotation));
                    blocks.Add(block);
                }
            }
        }
        // yatay ve dikey duvarlar
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.z; j++)
            {
                if (mapGrid[i][j] == CellType.HallwayBorderUnread)
                {
                    if (mapGrid[i + 1][j] == CellType.HallwayBorderUnread) // yatay duvarlar
                    {
                        int blockEnd = i;
                        for (int ii = i; mapGrid[ii][j] == CellType.HallwayBorderUnread; ii++)
                        {
                            mapGrid[ii][j] = CellType.HallwayBorder;
                            blockEnd = ii;
                        }
                        Vector3 pos = new((i + blockEnd) / 2.0f, hallwaySize.y / 2 + 1, j);
                        Vector3 size = new(blockEnd - i + 1, hallwaySize.y, 1);
                        Block block = new(cubePrefab, pos, size, hallwayWallMaterial);
                        blocks.Add(block);
                    }
                    else if (mapGrid[i][j + 1] == CellType.HallwayBorderUnread) // dikey duvarlar
                    {
                        int blockEnd = j;
                        for (int jj = j; mapGrid[i][jj] == CellType.HallwayBorderUnread; jj++)
                        {
                            mapGrid[i][jj] = CellType.HallwayBorder;
                            blockEnd = jj;
                        }
                        Vector3 pos = new(i, hallwaySize.y / 2 + 1, (j + blockEnd) / 2.0f);
                        Vector3 size = new(1, hallwaySize.y, blockEnd - j + 1);
                        Block block = new(cubePrefab, pos, size, hallwayWallMaterial);
                        blocks.Add(block);
                    }
                    else // tekli duvar
                    {
                        Vector3 pos = new(i, hallwaySize.y / 2 + 1, j);
                        Vector3 size = new(1, hallwaySize.y, 1);
                        Block block = new(cubePrefab, pos, size, hallwayWallMaterial);
                        blocks.Add(block);
                    }
                }
            }
        }
        // zemin ve tavan
        for (int i = 0; i < gridSize.x; i++) for (int j = 0; j < gridSize.z; j++) if (mapGrid[i][j] == CellType.HallwayBorder) mapGrid[i][j] = CellType.HallwayBorderUnread;
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.z; j++)
            {
                bool hallway = mapGrid[i][j] == CellType.HallwayBorderUnread || mapGrid[i][j] == CellType.HallwayUnread || mapGrid[i][j] == CellType.HallwayFieldUnread;
                if (hallway)
                {
                    Vector2Int topRight = new(i, j); // bottom left [i][j]
                    int top = j;
                    while (hallway) // BL -> BR
                    {
                        hallway = mapGrid[topRight.x + 1][topRight.y] == CellType.HallwayBorderUnread || mapGrid[topRight.x + 1][topRight.y] == CellType.HallwayUnread || mapGrid[topRight.x + 1][topRight.y] == CellType.HallwayFieldUnread;
                        if (hallway) topRight.x++;
                    }
                    hallway = true;
                    while (hallway) // BR -> TR
                    {
                        hallway = mapGrid[topRight.x][topRight.y + 1] == CellType.HallwayBorderUnread || mapGrid[topRight.x][topRight.y + 1] == CellType.HallwayUnread || mapGrid[topRight.x][topRight.y + 1] == CellType.HallwayFieldUnread;
                        if (hallway) topRight.y++;
                    }
                    hallway = true;
                    while (hallway) // BL -> TL
                    {
                        hallway = mapGrid[i][top + 1] == CellType.HallwayBorderUnread || mapGrid[i][top + 1] == CellType.HallwayUnread || mapGrid[i][top + 1] == CellType.HallwayFieldUnread;
                        if (hallway) top++;
                    }
                    if (top < topRight.y) topRight.y = top;
                    int iii = topRight.x;
                    while (iii != i)
                    {
                        iii = topRight.x;
                        hallway = true;
                        while (hallway) // TR -> TL
                        {
                            hallway = mapGrid[iii - 1][topRight.y] == CellType.HallwayBorderUnread || mapGrid[iii - 1][topRight.y] == CellType.HallwayUnread || mapGrid[iii - 1][topRight.y] == CellType.HallwayFieldUnread;
                            if (hallway) iii--;
                            if (iii == i) break;
                        }
                        if (iii == i) break;
                        else if (j < topRight.y) topRight.y--;
                        else
                        {
                            Debug.Log("bunun mümkün olduðunu bilmiyordum");
                            break;
                        }
                    }
                    for (int ii = i; ii <= topRight.x; ii++) for (int jj = j; jj <= topRight.y; jj++) switch (mapGrid[ii][jj])
                            {
                                case CellType.HallwayUnread:
                                    mapGrid[ii][jj] = CellType.Hallway;
                                    break;
                                case CellType.HallwayBorderUnread:
                                    mapGrid[ii][jj] = CellType.HallwayBorder;
                                    break;
                                case CellType.HallwayFieldUnread:
                                    mapGrid[ii][jj] = CellType.HallwayField;
                                    break;
                            }
                    Vector3 pos = new((topRight.x + i) / 2.0f, 0, (topRight.y + j) / 2.0f), size = new(topRight.x - i + 1, 1, topRight.y - j + 1);
                    Block block = new(cubePrefab, pos, size, hallwayFloorMaterial);
                    blocks.Add(block);
                    if (celling)
                    {
                        pos.y = hallwaySize.y + 1;
                        block = new(cubePrefab, pos, size, hallwayCellingMaterial);
                        blocks.Add(block);
                    }
                }
            }
        }

        PlaceBlocks();

    }
    private void PlaceBlocks()
    {
        //Debug.Log(blocks.Count);
        foreach(Block block in blocks)
        {
            GameObject go = Instantiate(block.prefab, block.pos, block.rotation, mapObject.transform);
            if (block.pos.y == 3) go.layer = 6;
            if (block.material) go.GetComponent<MeshRenderer>().material = block.material;
            if (block.size != Vector3.zero) go.transform.localScale = block.size;
            else go.transform.localScale = Vector3.one;
        }
    }

    private void SpawnPrefabs()
    {
        // collectable stones
        List<Room> rooms_ = new(rooms);
        int collectCount = 0;
        for (int i = 0; i < collectableCount; i++)
        {
            if (rooms_.Count <= 2) break;
            collectCount++;
            int index = random.Next(2, rooms_.Count);
            Vector3 pos = rooms_[index].pos;
            rooms_.RemoveAt(index);
            Instantiate(collectablePrefab, new Vector3(pos.x, 2, pos.z), collectablePrefab.transform.rotation, mapObject.transform);
        }
        player.stoneCount = collectCount;
        player.stonesCollected = 0;

    }
    private IEnumerator CreateNavMesh()
    {
        yield return new WaitForSeconds(0.1f);
        navMeshSurface.RemoveData();
        navMeshSurface.BuildNavMesh();

        GameObject wanderer = Instantiate(wandererPrefab, rooms[random.Next(2, rooms.Count)].pos, wandererPrefab.transform.rotation, mapObject.transform);
        GameObject sleeper = Instantiate(sleeperPrefab, rooms[random.Next(2, rooms.Count)].pos, sleeperPrefab.transform.rotation, mapObject.transform);
        GameObject screamer = Instantiate(screamerPrefab, rooms[random.Next(2, rooms.Count)].pos, sleeperPrefab.transform.rotation, mapObject.transform);

    }


    // ----- etc -----
    public void BornAgain()
    {
        //GameObject.FindWithTag("Player").GetComponent<player>().enabled = false;
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    private void PlaceTile(Vector3 pos, Vector3 size, GameObject tile, Material material)
    {
        GameObject go = Instantiate(tile, pos, Quaternion.identity);
        go.GetComponent<MeshRenderer>().material = material;
        go.transform.localScale = size;
    }
}
