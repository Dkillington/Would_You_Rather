using System;
using System.Diagnostics;
using Would_You_Rather;
using ChatGPTAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Functionality;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Would_You_Rather
{
    internal class Program
    {
        static Functionality.JSONSaving jsSave = new Functionality.JSONSaving();
        static Functionality.Text text = new Functionality.Text();
        static Functionality.Options ops = new Functionality.Options();
        static List<Question> questionSet = new List<Question>();
        static ChatGPTAccess.PythonScriptConnection pyScripts = new PythonScriptConnection();


        static string titleCredits = "Would You Rather                Author: David Killian         Release: 5/30/2024\n-----------------------------------------------------------------------------------";
        static string JSONFilePath = (Directory.GetCurrentDirectory() + @"\questions.json");
        static string JSONPythonsPath = (Directory.GetCurrentDirectory() + @"\pythonScriptInformation.json");
        static string emptyQuestionSetAlert = "You have 0 questions available! Add some to begin!"; // Message displayed when there are no questions available to play
        static int maximumChatGPTRequestedQuestions = 3;

        static async Task Main(string[] args)
        {
            // Load questions from file
            Load();

            while (true)
            {
                MainMenu();
            }
        }

        static async Task MainMenu()
        {
            bool pyScriptsPathsEmpty = pythonScriptPathsAreEmpty();

            Console.WriteLine(titleCredits);
            Console.WriteLine("[1] Play 'Would You Rather'\n");

            string gptText = "[2] Ask ChatGPT to add questions";
            if(pyScriptsPathsEmpty)
            {
                gptText += " (See Settings First!)";
            }
            Console.WriteLine($"{gptText}\n");
            Console.WriteLine("[3] Add your own question\n");
            Console.WriteLine("[4] See previous responses to questions\n");

            string settingsText = "[5] Settings";
            if (pyScriptsPathsEmpty)
            {
                settingsText += " (Python script paths need to be added here!)";
            }
            Console.WriteLine($"{settingsText}\n");
            Console.WriteLine("\n[6] Exit\n");

            string answer = Console.ReadLine().Trim().ToLower();
            Console.Clear();

            // Play
            if (answer == "1")
            {
                PlayGame();
            }
            // Let ChatGPT add questions
            else if (answer == "2")
            {
                await ChatGPTAddsQuestions();
            }
            // Add own question
            else if (answer == "3")
            {
                AddOwnQuestion();
            }
            // Look over previous questions
            else if (answer == "4")
            {
                Recap();
            }
            // Settings
            else if (answer == "5")
            {
                Settings();
            }
            // Quit the program
            else if (answer == "6" || answer == "exit")
            {
                Environment.Exit(0);
            }





            bool pythonScriptPathsAreEmpty()
            {
                if (string.IsNullOrWhiteSpace(pyScripts.pythonEXEPath) || string.IsNullOrWhiteSpace(pyScripts.pythonEXEPath))
                {
                    return true;
                }

                return false;
            }
        }
        static void Load()
        {
            jsSave.Load<List<Question>>(JSONFilePath, out questionSet);

            jsSave.Load<PythonScriptConnection>(JSONPythonsPath, out pyScripts, false);
        }
        static void SaveQuestions()
        {
            jsSave.Save(JSONFilePath, questionSet);
        }
        static async Task ChatGPTAddsQuestions()
        {

            ChatGPTAccess.Access a = new ChatGPTAccess.Access();
            string topic = GetTopic();
            int numOfQuestions = GetNum();

            string GPTQuestion = $"Return {numOfQuestions} 'Would You Rather' question(s) in this strict JSON format: " +
                            @"[{
                            """"option1"""": """"option 1 text"""",
                            """"option2"""": """"option 2 text"""",
                            """"category"""": """"topic here"""",
                            """"playerPicks"""": []
                            },{
                            """"option1"""": """"Another option 1 text"""",
                            """"option2"""": """"Another option 2 text"""",
                            """"category"""": """"topic here"""",
                            """"playerPicks"""": []
                            }]"

                            + $"Make the topic strictly related to {topic}, and make the 'topic here' text say {topic}. Make the choices short but hard. DON'T include the words 'Would You Rather'. Do not deviate from the JSON format given.";


            Console.WriteLine("Awaiting ChatGPT response . . .\n");
            string GPTResponse = a.AskGPT_Python_Script(GPTQuestion, pyScripts.pythonEXEPath, pyScripts.pythonChatGPTPath).GetAwaiter().GetResult();
            text.CancelInput();

            // If script returned an error, display it
            if(GPTResponse.Contains(ChatGPTAccess.Access.errorText))
            {
                Console.WriteLine(GPTResponse);
            }
            else
            {
                // If what was returned was a valid JSON string, parse and display it
                // Else display that it was an invalid response
                if (jsSave.IsValidJson(GPTResponse))
                {
                    AddGPTResponseToQuestionSet(GPTResponse);
                }
                else
                {
                    Console.WriteLine($"Invalid ChatGPT response returned:\n\n{GPTResponse}\nUnable to parse response, cancelling process. . . ");
                }
            }

            text.CancelInput();
            text.PressAnyKey();
            SaveQuestions();
            

            string GetTopic()
            {
                string topic = "";
                while(string.IsNullOrEmpty(topic))
                {
                    Console.WriteLine("Topic: \n");
                    topic = Console.ReadLine().Trim();
                    Console.Clear();
                }

                return topic;
            }
            int GetNum()
            {
                int num = (maximumChatGPTRequestedQuestions + 1);
                // While the picked number is greater than the max or below or equal to 0, keep retrying
                while (num > maximumChatGPTRequestedQuestions || num <= 0)
                {
                    Console.WriteLine($"Number of questions: (Maximum: {maximumChatGPTRequestedQuestions})\n");
                    int.TryParse(Console.ReadLine(), out num);
                    Console.Clear();
                }

                return num;
            }

            void AddGPTResponseToQuestionSet(string GPTResponse)
            {
                // Convert the string text into a list of Question objects
                try
                {
                    questionSet.AddRange(JsonConvert.DeserializeObject<List<Question>>(GPTResponse));
                    Console.WriteLine("GPT questions successfully added!");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deserializing the ChatGPT JSON data: " + ex.Message);
                    return;
                }
            }
        }
        static void AddOwnQuestion()
        {
            string topic = "";
            while (string.IsNullOrWhiteSpace(topic))
            {
                Console.Clear();
                Console.WriteLine("Write question topic below:\n");
                topic = Console.ReadLine();
            }

            string option1 = "";
            while (string.IsNullOrWhiteSpace(option1))
            {
                Console.Clear();
                Console.WriteLine("Write the first option below:\n");
                option1 = Console.ReadLine();
            }

            string option2 = "";
            while (string.IsNullOrWhiteSpace(option2))
            {
                Console.Clear();
                Console.WriteLine("Write the second option below:\n");
                option2 = Console.ReadLine();
            }

            Console.Clear();
            questionSet.Add(new Question(new List<PlayerPick>(), option1, option2, topic));
            SaveQuestions();
        }
        static void PlayGame()
        {
            // If there are no questions available, don't play
            if(questionSet.Count <= 0)
            {
                Console.WriteLine(emptyQuestionSetAlert);
                text.PressAnyKey();
            }
            else
            {
                bool playing = true;
                while(playing)
                {
                    Console.WriteLine("Play random questions or by question category?\n");
                    Console.WriteLine("[1] Random");
                    Console.WriteLine("[2] By Category\n");
                    string answer = Console.ReadLine();
                    Console.Clear();

                    // Play random questions
                    if (answer == "1")
                    {
                        for (int i = 0; i < questionSet.Count;)
                        {
                            bool deleted = AskQuestion(ref i, questionSet[i], questionSet);

                            // Check if you want to delete a question from the set
                            if (deleted)
                            {
                                questionSet.RemoveAt(i);
                            }
                            else
                            {
                                // Only increment i if you're not deleting a question
                                i++;
                            }

                            // GENIUS FEATURE imo
                            // This feature is to exit the deck from within the 'AskQuestion' method.
                            // In the method, it references index count directly. If the player wants to exit, it sets the index to a negative number (Impossible to achieve negative questions), and this catches it on the way out and sets it to the count so the loop is exited
                            if(i >= questionSet.Count || i < 0)
                            {
                                i = questionSet.Count;
                            }

                            SaveQuestions();
                        }

                        playing = false;
                    }
                    // Play questions by category
                    else if (answer == "2")
                    {
                        // This adds every unique category to a dictionary, with a number as a key
                        // Prevents duplicate categories being entered, and allows for selecting specific categories
                        Dictionary<string, string> numToCategoryDict = new Dictionary<string, string>();
                        int counter = 0;
                        foreach (Question question in questionSet)
                        {
                            if (!numToCategoryDict.ContainsValue(question.category))
                            {
                                counter++;
                                numToCategoryDict.Add(counter.ToString(), question.category);
                            }
                        }

                        // Waits for player to select a valid category
                        // Player enters a number, it checks the dictionary to see if that number is a key, if it is returns a valid category to play
                        string pickedCategory = null;
                        while (pickedCategory == null)
                        {
                            Console.Clear();
                            foreach (KeyValuePair<string, string> pair in numToCategoryDict)
                            {
                                Console.WriteLine($"[{pair.Key}] {pair.Value}");
                            }
                            Console.WriteLine();

                            answer = Console.ReadLine();
                            Console.Clear();

                            if (numToCategoryDict.ContainsKey(answer))
                            {
                                pickedCategory = numToCategoryDict[answer];
                            }
                        }


                        // Creates a temporary list of questions that are strictly related to the picked category
                        // Pulls from the main list of questions, filtering by category
                        List<Question> questions = new List<Question>();
                        foreach (Question question in questionSet)
                        {
                            if (question.category == pickedCategory)
                            {
                                questions.Add(question);
                            }
                        }

                        // Now asks each question, using the temporary list
                        for (int i = 0; i < questions.Count;)
                        {
                            bool deleted = AskQuestion(ref i, questions[i], questions);

                            // Check if you want to delete a question from the set
                            if (deleted)
                            {
                                questionSet.Remove(questions[i]);
                                questions.RemoveAt(i);
                            }
                            else
                            {
                                // Only increment i if you're not deleting a question
                                i++;
                            }

                            // The trick mentioned above ^
                            if (i >= questionSet.Count || i < 0)
                            {
                                i = questionSet.Count;
                            }


                            SaveQuestions();
                        }

                        playing = false;
                    }
                }
            }

            // Functionality for asking a question
            static bool AskQuestion(ref int currentIndex, Question selectedQuestion, List<Question> deck)
            {
                // Returns bool if deleted or not

                // Repeatedly ask player to choose a valid choice until they do
                bool notAnswered = true;
                while (notAnswered)
                {
                    Console.WriteLine($"Question {currentIndex + 1} of {deck.Count}     [Topic: {selectedQuestion.category}]\n------------------------------------");
                    selectedQuestion.Display();

                    Console.WriteLine("[Del] Delete Question");
                    Console.WriteLine("[Exit] Exit\n");

                    string answer = Console.ReadLine().Trim().ToLower();

                    if (answer == "1")
                    {
                        selectedQuestion.playerPicks.Add(new PlayerPick(DateTime.Now.ToString(), selectedQuestion.option1));
                        notAnswered = false;
                    }
                    else if (answer == "2")
                    {
                        selectedQuestion.playerPicks.Add(new PlayerPick(DateTime.Now.ToString(), selectedQuestion.option2));
                        notAnswered = false;
                    }
                    else if (answer == "del" || answer == "delete")
                    {
                        notAnswered = false;
                        Console.Clear();
                        return true;
                    }
                    else if (answer == "back" || answer == "exit")
                    {
                        notAnswered = false;
                        currentIndex = 100;
                    }
                    else
                    {

                    }

                    Console.Clear();
                }

                // Ensure PlayerPicks only keeps the nth newest picks
                KeepListOfPicksWithinNumber();
                return false;

                void KeepListOfPicksWithinNumber(int maxNumberOfPicks = 2)
                {
                    // If the number of picks is equal to the maximum value, remove the first pick (The newest entry)
                    if (selectedQuestion.playerPicks.Count > maxNumberOfPicks)
                    {
                        selectedQuestion.playerPicks.Remove(selectedQuestion.playerPicks[0]);
                    }
                }

            }
        }
        static void Recap()
        {
            if(questionSet.Count > 0)
            {
                int indexCounter = 0;
                foreach (Question question in questionSet)
                {
                    indexCounter++;

                    Console.WriteLine($"[RECAP]     {indexCounter} of {questionSet.Count}\n-------------------------------------------------------");

                    question.ShowPlayerChoices();
                    Console.WriteLine($"\n(Press 'Enter' or 'Esc' to exit)\n");
                    char character = Console.ReadKey().KeyChar;
                    Console.Clear();

                    if (character == ((char)ConsoleKey.Enter) || character == ((char)ConsoleKey.Escape))
                    {
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine(emptyQuestionSetAlert);
                text.PressAnyKey();
            }    
        }
        static void Settings()
        {
            bool inMenu = true;
            while(inMenu)
            {
                Dictionary<string, Action> actionOps = new Dictionary<string, Action>();
                actionOps.Add("Configure Python script locations", ConfigurePyLocations);
                actionOps.Add("RESET (Delete all questions)", RESET);
                ops.ActionOptions("Settings", actionOps, ref inMenu).Invoke();
            }


            void ConfigurePyLocations()
            {
                pyScripts = pyScripts.ReturnFile();
                SaveJSONInfo();
                void SaveJSONInfo()
                {
                    jsSave.Save(JSONPythonsPath, pyScripts);
                }
            }

            void RESET()
            {
                bool confirm = text.AskToConfirm("Are you sure you want to fully reset cards?");
                if (confirm)
                {
                    Console.WriteLine($"{questionSet.Count} questions removed!");
                    questionSet.Clear();
                    SaveQuestions();
                    text.PressAnyKey();
                }
                else
                {

                }
            }



        }
    }
}