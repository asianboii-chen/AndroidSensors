using NetworkUtil;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TizenSensor.lib
{
	public class Client
	{
		public const int Port = 6912;

		public static void Connect(string serverAddr, Action<Client> onConnect)
		{
			Networking.ConnectToServer(
				state =>
				{
					if (state.ErrorOccured) onConnect(null);
					else onConnect(new Client(state.TheSocket));
				},
				"192.168.0.56",
				6912
			);
		}

		protected Client(Socket socket)
		{
			this.socket = socket;
		}

		protected Socket socket;

		public bool Send(string data)
		{
			return Networking.Send(socket, data);
		}
	}
}
