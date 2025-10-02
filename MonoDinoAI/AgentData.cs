using System.Text.Json.Serialization;

namespace MonoDinoAI
{
    // Data sent to /get_action
    public class StateRequest
    {
        [JsonPropertyName("State")]
        public float[] State { get; set; }
    }
    // Data sent to /train
    public class Experience
    {
        [JsonPropertyName("OldState")]
        public float[] OldState { get; set; }

        [JsonPropertyName("Action")]
        public int Action { get; set; }

        [JsonPropertyName("Reward")]
        public float Reward { get; set; }

        [JsonPropertyName("NewState")]
        public float[] NewState { get; set; }

        [JsonPropertyName("IsDone")]
        public bool IsDone { get; set; }
    }
}
