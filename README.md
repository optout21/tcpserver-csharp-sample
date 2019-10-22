# TcpServer in C# Example

A sample networking implementation, in C#, including sample node implementation.

## Prerequisites
* .NET Core

## Usage

    git clone https://github.com/catenocrypt/tcpserver-csharp-sample.git
    cd tcpserver-csharp-sample
    dotnet build
    dotnet run -p server

From another console:

    dotnet run -p client

## Notes

* Connections are TCP connections
* Messages are encoded simple text-based, variable-length, using terminators and separators.
* Transitive peer discovery is done (in node)

## Executables 

* tcp-libuv-server: Listens on port 5000 (or tries a few next ones if taken), and accepts connections.
* tcp-libuv-client: Tries to connect to localhost:5000 and a few next ports, and sends Handshake and a few Ping messages.
* tcp-libuv-node: Acts as a P2P peer: Listens on port 5000 (or tries a few next ones if taken), tried to connect to localhost:5000 and a few next ports.  Performs handshakes, periodic Pings.  Also sends periodically the connected peers to other peers.
