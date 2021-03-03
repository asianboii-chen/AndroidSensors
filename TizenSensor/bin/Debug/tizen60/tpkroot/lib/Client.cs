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

		public static void Connect(string serverAddr, Action<Client> onConnected)
		{
			Networking.ConnectToServer(
				state =>
				{
					if (state.ErrorOccured) onConnected(null);
					else onConnected(new Client(state.TheSocket));
				},
				serverAddr,
				Port
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
