using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp
{
	public partial class Form1 : Form
	{
		HubConnection connection;
		Message message;
		Random rnd;
		public Form1()
		{
			InitializeComponent();

			connection = new HubConnectionBuilder()
				.WithUrl("http://192.168.1.10:5051/myHub")
				.WithAutomaticReconnect()
				.Build();

			rnd = new Random();

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
					await connection.InvokeCoreAsync("GetMessages", args: new[] { new List<Message>() });
				});
				Task.WaitAll(ReconnectedClient);
			}
			message = new Message($"user_{rnd.Next(60000)}");
		}//ctor

		private void OnReceiveMessage(Message message)
		{
			richTextBox1.Text += $"[{message.Time.ToShortTimeString()}] {message.Name}: {message.Text}\n";
		}

		private void OnReconnectedChat(List<Message> messages)
		{
			foreach (var message in messages)
				richTextBox1.Text += $"[{message.Time.ToShortTimeString()}] {message.Name}: {message.Text}\n";
				//Console.WriteLine($"[{message.Time.ToShortTimeString()}] {message.Name} {message.Text}");		
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(textBox1.Text))
			{
				message.Text = textBox1.Text;
				Task.Run(async () =>
				{
					await connection.InvokeCoreAsync("SendMessage", args: new[] { message });
				});
			}
		}
	}//class

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
}//namespace
