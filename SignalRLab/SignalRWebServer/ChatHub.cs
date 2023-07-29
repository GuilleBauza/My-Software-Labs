using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace SignalRWebServer;

public class ChatHub: Hub
{
	public static List<Message> Messages = new List<Message>();

	public async Task SendMessage(Message message)
	{
		HandleLastestMessages(message);
		await Clients.All.SendAsync("MessageReceived", message);
	}

	public async Task GetMessages(List<Message> nothing)
	{
		Debug.WriteLine("This Got Called");
		
		await Clients.Caller.SendAsync("ReconnectedClient", Messages);
	}

	private void HandleLastestMessages(Message msg)
	{
		Messages.Add(msg);
		if(Messages.Count > 500)
		{
			Messages.RemoveAt(0);
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

    public Message()
    {
			
    }
}//class