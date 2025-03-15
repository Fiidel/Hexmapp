namespace RelayServer.Rooms
{
    public class RoomCodeGenerator : IRoomCodeGenerator
    {
        public string GenerateRoomCode()
        {
            var symbolArray = "ABCDEFGHIJKLMNOPQRSTUVXYZ0123456789";
            int codeLength = 8;
            char[] roomCode = new char[codeLength];
            Random random = new Random();
            for (int i = 0; i < codeLength; i++)
            {
                roomCode[i] = symbolArray[random.Next(0, symbolArray.Length)];
            }
            return new string(roomCode);
        }
    }
}
