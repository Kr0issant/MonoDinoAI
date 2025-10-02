using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDinoAI
{
    public class AgentClient
    {
        public bool IsAgentThinking { get; set; } = false;
        public int PendingAction = 0;

        private readonly HttpClient _client;
        private readonly string _baseUrl = "http://127.0.0.1:5000";

        private float[] lastState;
        private int lastAction;
        private float lastReward;
        private float[] currentState;
        private bool isGameOver;

        public AgentClient()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromMilliseconds(500);
        }

        public async Task<int> GetActionAsync(float[] stateArray)
        {
            var request = new StateRequest { State = stateArray };
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync($"{_baseUrl}/get_action", content);
                response.EnsureSuccessStatusCode();

                var actionString = await response.Content.ReadAsStringAsync();
                if (int.TryParse(actionString, out int action))
                {
                    return action;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"[Agent Error] Could not connect to agent: {e.Message}");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Agent Error] Unexpected error in GetAction: {e.Message}");
            }
            return 0;
        }

        public async Task SendExperienceAsync(Experience experience)
        {
            var jsonContent = JsonSerializer.Serialize(experience);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync($"{_baseUrl}/train", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Agent Error] Failed to send experience: {e.Message}");
            }
        }
        public void StartAgentDecisionTask()
        {
            IsAgentThinking = true;

            Task.Run(async () =>
            {
                World.UpdateLastScore();
                lastState = World.GetCurrentState();
                lastAction = await GetActionAsync(lastState);

                Interlocked.Exchange(ref PendingAction, lastAction);

                isGameOver = World.IsGameOverActual;
                lastReward = World.CalculateReward(isGameOver, lastAction);
                currentState = World.GetCurrentState();

                var experience = new Experience
                {
                    OldState = lastState,
                    Action = lastAction,
                    Reward = lastReward,
                    NewState = currentState,
                    IsDone = isGameOver
                };

                await SendExperienceAsync(experience);

                IsAgentThinking = false;
            });
        }
    }
}
