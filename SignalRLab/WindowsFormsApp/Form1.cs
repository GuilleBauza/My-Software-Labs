using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp
{
	public partial class Form1 : Form
	{
		HubConnection connection;
		ChatMessage message;
		Random rnd;
		public Form1()
		{
			InitializeComponent();

			connection = new HubConnectionBuilder()
				.WithUrl("http://192.168.1.10:5167/TuDemandaAppChat")
				.WithAutomaticReconnect()
				.Build();

			rnd = new Random();

			connection.On<ChatMessage>("MessageReceived", OnReceiveMessage);
			connection.On<List<ChatMessage>>("ReconnectedClient", OnReconnectedChat);

			var connected = Task.Run(async () =>
			{
				await connection.StartAsync();
			});

			Task.WaitAll(connected);

			Debug.WriteLine($"ConnectionId: {connection.State}");

			if (connection.State == HubConnectionState.Connected)
			{
				var ReconnectedClient = Task.Run(async () =>
				{
					await connection.InvokeCoreAsync("GetMessages", args: new[] { new List<Message>() });
				});
				Task.WaitAll(ReconnectedClient);
			}
			message = new ChatMessage($"user_{rnd.Next(60000)}");
		}//ctor

		private void OnReceiveMessage(ChatMessage message)
		{
			richTextBox1.Text += $"[{message.time.ToShortTimeString()}] {message.name}: {message.text}\n";
		}

		private void OnReconnectedChat(List<ChatMessage> messages)
		{
			foreach (var message in messages)
				richTextBox1.Text += $"[{message.time.ToShortTimeString()}] {message.name}: {message.text}\n";
				//Console.WriteLine($"[{message.Time.ToShortTimeString()}] {message.Name} {message.Text}");		
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(textBox1.Text))
			{
				message.text = textBox1.Text;
				Task.Run(async () =>
				{
					await connection.InvokeCoreAsync("SendMessage", args: new[] { message });
				});
			}
		}
	}//class

	public class ChatMessage
	{
		public const string ACTION_MESSAGE_RECEIVED = "MessageReceived";
		public const string ACTION_CONNECTED_CLIENT = "ReconnectedClient";
		public const string INVOKE_METHOD_GETMESSAGES = "GetMessages";
		public const string INVOKE_METHOD_SENDMESSAGE = "SendMessage";
		public const string CHAT_HUB_ID = "TuDemandaAppChat";

		public string userId { get; set; }
		public string avatar { get; set; } = "logo.png";
		public string name { get; set; }
		public string text { get; set; }
		public DateTime time { get; set; } = DateTime.Now;

		public string TimeText
		{
			get { return $"{time.ToShortTimeString()}"; }
		}

        public ChatMessage()
        {
        }

        public ChatMessage(string name)
        {
            this.name = name;
        }
    }
}//namespace
