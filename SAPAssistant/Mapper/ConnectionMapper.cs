using SAPAssistant.Models;

namespace SAPAssistant.Mapper
{
    public static class ConnectionMapper
    {
        public static List<ConnectionDTO> FromRawList(ExecutorResponse rawList)
        {
            var result = new List<ConnectionDTO>();

            foreach (var item in rawList.Data)
            {
                foreach (var kvp in item)
                {
                    var conn = kvp.Value;
                    conn.ConnectionId = kvp.Key;
                    result.Add(conn);
                }
            }

            return result;
        }
    }

}
