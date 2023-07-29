using Microsoft.AspNetCore.SignalR.Client;
using System.Xml.Linq;

namespace ConsoleAppCore7
{
	internal class Program
	{
		static Message message;
		static void Main(string[] args)
		{

			try
			{
				//var connection = new HubConnection();
				var connection = new HubConnectionBuilder()
					.WithUrl("http://192.168.1.10:5051/myHub")
					.WithAutomaticReconnect()
					.Build();

				Random rnd = new Random();

				connection.On<Message>("MessageReceived", OnReceiveMessage);
				connection.On<List<Message>>("ReconnectedClient", OnReconnectedChat);

				var connected = Task.Run(async () =>
				{
					await connection.StartAsync();
				});

				Task.WaitAll(connected);


				if (connection.State == HubConnectionState.Connected)
				{
					var ReconnectedClient = Task.Run(async () =>
					{
						await connection.InvokeCoreAsync("GetMessages", args: new[] { new List<Message>()});
					});
					Task.WaitAll(ReconnectedClient);


					message = new Message($"user_{rnd.Next(60000)}");

					string msg = string.Empty;
					do
					{
						Console.WriteLine();
						msg = Console.ReadLine();
						
						if (msg.Replace("Enter Message: ", string.Empty) != "exit")
						{
							message.Text = msg;
							Task.Run(async () =>
							{
								await connection.InvokeCoreAsync("SendMessage", args: new[] { message });
							});
						}
					} while (msg != "exit");
				}
				else
				{

				}
			}
			catch (Exception ex)
			{
				Console.Write(ex);
				Console.ReadKey();
			}
			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		private static void OnReceiveMessage(Message message)
		{
			if (message.Name != Program.message.Name)
				Console.WriteLine($"[{message.Time.ToShortTimeString()}] {message.Name} {message.Text}");
		}

		private static void OnReconnectedChat(List<Message> messages)
		{
			foreach (var message in messages)
			{
				Console.WriteLine($"[{message.Time.ToShortTimeString()}] {message.Name} {message.Text}");
			}	
		}

	}

	public class Message
	{
		public string Name { get; set; }
		public string Text { get; set; }
		public DateTime Time { get; set; }

		public Message(string name, string text = "")
		{
			Name = name;
			Text = text;
			Time = DateTime.Now;
		}
	}//class
}