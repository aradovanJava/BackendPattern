using System.Text.Json;
using System.Text.Json.Serialization;

namespace Back.Models
{
    public class UserRoleUpdate
    {
        public UserRoleUpdate(int id, int role)
        {
            Id = id;
            Role = role;
        }

        public int Id { get; set; }
        public int Role { get; set; }
    }

    public class UserRoleUpdateConverter : JsonConverter<UserRoleUpdate>
    {
        private static readonly HashSet<int> ValidRoles = new HashSet<int> { 1, 2, 3, 4 };

        public override UserRoleUpdate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int id = 0, role = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName == "id")
                        id = reader.GetInt32();
                    else if (propertyName == "role")
                        role = reader.GetInt32();
                }
            }

            if (!ValidRoles.Contains(role))
                throw new JsonException("Invalid role value.");

            return new UserRoleUpdate(id, role);
        }

        public override void Write(Utf8JsonWriter writer, UserRoleUpdate value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("id", value.Id);
            writer.WriteNumber("role", value.Role);
            writer.WriteEndObject();
        }
    }

    public static class UserRoleUpdateSerializer
    {
        private static readonly JsonSerializerOptions Options;
        private static string PathToJson = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "JsonSer");
        private static string FileName = "useri.json";

        static UserRoleUpdateSerializer()
        {
            Options = new JsonSerializerOptions();
            Options.Converters.Add(new UserRoleUpdateConverter());
        }

        public static string SerializeUserRoleUpdates(List<UserRoleUpdate> userRoleUpdates)
        {
            return JsonSerializer.Serialize(userRoleUpdates, Options);
        }

        public static List<UserRoleUpdate> DeserializeUserRoleUpdates(string json)
        {
            return JsonSerializer.Deserialize<List<UserRoleUpdate>>(json, Options);
        }

        public static void WriteToFile(List<UserRoleUpdate> userRoleUpdates)
        {
            string json = SerializeUserRoleUpdates(userRoleUpdates);
            string filePath = Path.Combine(PathToJson, FileName);

            File.WriteAllText(filePath, json);
        }

        public static List<UserRoleUpdate> ReadFromFile()
        {
            string filePath = Path.Combine(PathToJson, FileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file '{filePath}' does not exist.");

            string json = File.ReadAllText(filePath);
            File.WriteAllText(filePath, string.Empty);

            return DeserializeUserRoleUpdates(json);
        }
    }
}
