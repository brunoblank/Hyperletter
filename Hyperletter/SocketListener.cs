using System;
using System.Net;
using System.Net.Sockets;
using Hyperletter.Channel;

namespace Hyperletter {
    internal class SocketListener : IDisposable {
        private readonly Binding _binding;
        private readonly AbstractHyperSocket _hyperSocket;
        private bool _disposed;
        private TcpListener _listener;

        public SocketListener(AbstractHyperSocket hyperSocket, Binding binding) {
            _hyperSocket = hyperSocket;
            _binding = binding;
        }

        public void Dispose() {
            _disposed = true;
        }

        public event Action<InboundChannel> IncomingChannel;

        public void Start() {
            _listener = new TcpListener(_binding.IpAddress, _binding.Port);
            _listener.Start();
            StartListen();
        }

        private void StartListen() {
            if(_disposed)
                return;

            _listener.BeginAcceptTcpClient(EndAccept, null);
        }

        private void EndAccept(IAsyncResult res) {
            StartListen();

            if(_disposed)
                return;

            TcpClient tcpClient = _listener.EndAcceptTcpClient(res);
            tcpClient.NoDelay = true;
            tcpClient.LingerState = new LingerOption(true, 1);
            Binding binding = GetBinding(tcpClient.Client.RemoteEndPoint);
            var boundChannel = new InboundChannel(_hyperSocket, tcpClient, binding);
            IncomingChannel(boundChannel);
        }

        private Binding GetBinding(EndPoint endPoint) {
            var ipEndpoint = ((IPEndPoint) endPoint);
            return new Binding(ipEndpoint.Address, ipEndpoint.Port);
        }
    }
}