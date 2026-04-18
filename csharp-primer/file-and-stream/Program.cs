using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.IO.IsolatedStorage;
using System.IO.Pipes;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Text;
using static System.Net.WebRequestMethods;

namespace _01_file_and_stream
{
    internal class StreamAdapters
    {
        /// <summary>
        ///  Three types of streams:
        ///  Backing store: 
        ///  <list type="bullet">
        ///     <item><para>
        ///     A stream that provides access to a backing store, 
        ///     such as a file or memory. Examples include 
        ///     FileStream, MemoryStream, and NetworkStream.
        ///     </para></item>
        ///     <item><para>
        ///     Decorator: A stream that adds additional functionality
        ///     to an existing stream. Examples include 
        ///     BufferedStream, CryptoStream, and GZipStream.</para></item>
        ///     <item><para>
        ///     Adapter: A stream that adapts an existing stream to a different interface. 
        ///     Examples include 
        ///     StreamReader, StreamWriter, BinaryReader, and BinaryWriter.
        ///     </para></item>
        ///  </list>
        /// </summary>
        /// 

        /// <summary>
        /// In C#, a Stream is an abstract base class that represents a sequence of bytes. 
        /// It serves as a unified interface for transferring data between your application 
        /// and various external sources or destinations, such as 
        /// files, memory, or network sockets
        /// </summary>


        internal static void StreamClassCanBeUsedToConnectToMemoryStream()
        {
            byte[] buffer = new byte[1024];
            byte[] readout = new byte[1024];
            byte[] data = Encoding.UTF8.GetBytes("Hello, World!");


            Stream stream = new MemoryStream(buffer);

            stream.Write(data, 0, data.Length);
            // if you want to read from the stream after writing,
            // you need to set the position back to the beginning of the stream.
            // Otherwise, you will be reading from the end of the stream and get 0 bytes read.
            stream.Position = 0;
            //stream.Close(); can not read after close.
            //ObjectDisposedException will be thrown if you try to
            //read or write after close.

            int bytesRead = stream.Read(readout, 0, buffer.Length);

            Console.WriteLine(stream.Position); // stream class provides a position property to track the current position in the stream
            Console.WriteLine(bytesRead); // stream class provides a read method that returns the number of bytes read
            Console.WriteLine("The string is:" + Encoding.UTF8.GetString(readout, 0, bytesRead)); // stream class provides a write method that writes data to the stream
        }

        internal static void StreamClassCanBeUsedToConnectToFileStream() 
        {
            // Example of using a FileStream to read from a file
            using (Stream stream = new FileStream("example.txt", FileMode.Open, FileAccess.Read))
            {
                // classic approach with byte array buffer
                //  new byte[1024] allocates on the managed heap, which adds GC pressure.
                byte[] buffer = new byte[1024];                
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"Read {bytesRead} bytes from file.");
            }

            using (Stream stream = new FileStream("example.txt", FileMode.Open, FileAccess.Read))
            {
                //  Size limit: stackalloc is only safe for small,
                //  bounded buffers (typically ≤1 KB, or behind a stackalloc + heap fallback pattern).
                //  A 10 MB stackalloc will blow the stack.
                Span<byte> buffer = stackalloc byte[1024];
                //  the newer span-based overload, which avoids extra bounds-check overhead in some paths
                //  and enables zero-copy slicing.
                //  span can be user for write operation as well.
                int bytesRead = stream.Read(buffer);
                Console.WriteLine($"Read {bytesRead} bytes from file.");
            }
        }

        /// <summary>
        /// why BinaryReader's ReadBytes is better than Stream's Read method?
        /// Because BinaryReader's ReadBytes method will return all the bytes available in the stream
        /// if the number of bytes to read is greater than the number of bytes available in the stream,
        /// while Stream's Read method will return 0 bytes read if you try to read more bytes than available
        /// in the stream.
        /// </summary>
        internal static void BinaryReaderClassReview()
        {
            byte[] buffer = new byte[1024];
            byte[] readout;
            byte[] data = Encoding.UTF8.GetBytes("Hello, World!");

            Stream stream = new MemoryStream(buffer);
            stream.Write(data, 0, data.Length);

            // even BinaryReader needs to set the position back to the beginning of the stream
            // before reading, otherwise it will read from the end of the stream and get 0 bytes read.
            stream.Position = 0;

            BinaryReader binaryReader = new BinaryReader(stream);
            // BinaryReader class provides a ReadBytes method that reads a specified number of bytes from the stream and returns them as a byte array.
            // If the number of bytes to read is greater than the number of bytes available in the stream, it will return all the bytes available in the stream.
            readout = binaryReader.ReadBytes(10000);

            Console.WriteLine("The string is:" + Encoding.UTF8.GetString(readout, 0, readout.Length));
        }

        internal static void MemoryStreamWithFixedAndDynamicBufferHasDifferentCharachteristics()
        {
            // Example of using a MemoryStream to read/write data in memory
            byte[] buffer = new byte[5];
            byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
            // MemoryStream can be created with a fixed buffer, which does not allow resizing.
            using (Stream stream = new MemoryStream(buffer))
            {
                // this line throws an exception because the buffer is only 5 bytes long,
                // but we are trying to write 13 bytes to it.
                // The MemoryStream does not automatically resize the buffer when you write to it,
                // so you need to make sure that the buffer is large enough to hold the data you want to
                // write.
                //stream.Write(data, 0, data.Length); 
                stream.Position = 0; // Reset position to read from the beginning
                
                byte[] readout = new byte[1024];
                int bytesRead = stream.Read(readout, 0, buffer.Length);
                Console.WriteLine($"Read {bytesRead} bytes from memory stream.");
                Console.WriteLine("The string is:" + Encoding.UTF8.GetString(readout, 0, bytesRead));
            }

            // MemoryStream can also be created without a fixed buffer, which allows it to resize as needed.
            using (Stream stream = new MemoryStream())
            {                
                stream.Write(data, 0, data.Length); 
                stream.Position = 0; // Reset position to read from the beginning

                byte[] readout = new byte[1024];                
                int bytesRead = stream.Read(readout, 0, buffer.Length);
                Console.WriteLine($"Read {bytesRead} bytes from memory stream.");
                Console.WriteLine("The string is:" + Encoding.UTF8.GetString(readout, 0, bytesRead));
            }
        }

        /// <summary>
        /// IsolatedStorageStream is a special type of stream that provides a
        /// virtual file system for storing data in a user-specific and application-specific
        /// manner.
        /// In modern applications, IsolatedStorageFileStream should not prefered, 
        /// instead, use the standard file system APIs with appropriate user directories
        /// (like AppData) for storing application data.
        /// It is often used for storing user settings, preferences, or other small pieces of
        /// data that need to be persisted across application sessions.
        /// Example of using IsolatedStorageStream to write and read data
        /// </summary>
        internal static void IsolatedStorageStreamExample()
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                using (var stream = new IsolatedStorageFileStream("example.txt", FileMode.Create, isoStore))
                {
                    byte[] data = Encoding.UTF8.GetBytes("Hello, Isolated Storage!");
                    stream.Write(data, 0, data.Length);
                }
                using (var stream = new IsolatedStorageFileStream("example.txt", FileMode.Open, isoStore))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"Read {bytesRead} bytes from isolated storage.");
                    Console.WriteLine("The string is:" + Encoding.UTF8.GetString(buffer, 0, bytesRead));
                }
            }
        }

        /// <summary>
        /// NetworkStream is a stream that provides access to data over a network connection.
        /// It is typically used in conjunction with TcpClient or TcpListener for sending and receiving data over TCP/IP.
        /// Note: This example requires a running TCP server to connect to, so it may not work as-is without a server.
        /// When to use it
        ///   Custom TCP protocols: you're writing a client/server with your own wire format (game servers, IoT devices, telnet-like tools, message brokers).
        ///   Interop with non - HTTP services: SMTP, IMAP, Redis, MySQL wire protocol, etc., when no higher - level library exists.
        ///   Wrapping with SslStream for TLS over a raw socket.
        ///   
        /// When not to use it
        /// <list type="bullet">
        /// <item>HTTP / REST / gRPC — use HttpClient or gRPC client; they handle framing, headers, retries.</item>
        /// <item>UDP — use UdpClient/Socket directly; NetworkStream requires a connected stream socket.</item>
        /// <item>File or in-memory I/O — use FileStream / MemoryStream.</item>
        /// </list>
        /// </summary>
        internal static void NetworkStreamExample()
        {
            // TcpListener binds a socket and waits for inbound connections.
            // Loopback keeps the example off the network interface — nothing
            // leaves the box, no firewall prompt, no port-scanner visibility.
            int port = 12345;
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
            listener.Start();
            Console.WriteLine($"[SERVER] Listening on 127.0.0.1:{port}");

            // Server task: accept one client, read its request, echo a reply.
            Task serverTask = Task.Run(() =>
            {
                // AcceptTcpClient blocks until a client connects. The returned
                // TcpClient owns the accepted socket — disposing it closes that
                // per-connection socket, not the listener.
                using var server = listener.AcceptTcpClient();
                Console.WriteLine("[SERVER] Client connected");

                using var stream = server.GetStream();

                // One Read call returns whatever bytes are currently available
                // (could be a partial message). For a real protocol you need
                // length-prefix framing or a delimiter; a single short Write
                // from the client typically arrives as a single Read here.
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"[SERVER] Received: {received}");

                byte[] reply = Encoding.UTF8.GetBytes($"Echo: {received}");
                stream.Write(reply, 0, reply.Length);
                Console.WriteLine("[SERVER] Reply sent");
            });

            // Client task: connect, send a string, read the reply.
            Task clientTask = Task.Run(() =>
            {
                // The TcpClient ctor performs a blocking connect. If the
                // listener weren't already started, this would race — hence
                // listener.Start() runs on the main thread before Task.Run.
                using var client = new System.Net.Sockets.TcpClient("localhost", port);
                Console.WriteLine("[CLIENT] Connected to server");

                using var stream = client.GetStream();

                byte[] dataToSend = Encoding.UTF8.GetBytes("Hello, Server!");
                stream.Write(dataToSend, 0, dataToSend.Length);
                Console.WriteLine("[CLIENT] Request sent");

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"[CLIENT] Received: {Encoding.UTF8.GetString(buffer, 0, bytesRead)}");
            });

            Task.WaitAll(serverTask, clientTask);
            listener.Stop();
            Console.WriteLine("[COMPLETE] NetworkStream communication finished\n");
        }


        /// <summary>
        ///   What "synchronized" actually means here                                                                                                                                                                                                                    - Mutual exclusion, not async.Only one thread at a time can be inside any method on the wrapper.Other threads calling Read/Write block on the monitor until the current call returns.
        /// It's roughly equivalent to:
        ///   lock (syncRoot) { return inner.Read(buffer, offset, count); }
        /// 
        /// The lock uses the wrapper instance as the sync root, 
        /// so all operations share one lock — read vs write are not independent.
        ///   
        /// What it does not do
        /// <list type="bullet">
        ///   <item>It does not make the I/O non-blocking.If the underlying FileStream.Read takes 2 seconds to hit the disk, every other thread waits those 2 seconds.</item>
        ///   <item>It does not help with ReadAsync/WriteAsync — async methods go through the base class and may not acquire the same lock, so mixing sync and async on a Synchronized wrapper is not safe.</item>
        ///   <item>It does not coordinate the stream position.Thread A can Seek then Read; between those two calls Thread B can Seek too.Each call is atomic, but sequences of calls are not.</item>
        ///   <item>It does not protect the underlying stream from code that still holds the original reference — if you synchronize stream but then also write to stream directly, you've bypassed the lock.</item>
        /// </list>
        /// Conclusion: Stream.Synchronized is a very narrow tool that only makes individual Read/Write calls thread-safe. It does not make the stream itself thread-safe for complex operations or async usage.
        /// </summary>
        internal static void ThreadSafeStreamAdapter()
        {
            // Test WITHOUT synchronization (may cause issues)
            Console.WriteLine("Testing WITHOUT Synchronization:");
            TestStreamConcurrency(useSynchronized: false);

            // Test WITH synchronization (thread-safe)
            Console.WriteLine("\nTesting WITH Synchronization:");
            TestStreamConcurrency(useSynchronized: true);
        }

        private static void TestStreamConcurrency(bool useSynchronized)
        {
            string filename = useSynchronized ? "test_safe.txt" : "test_unsafe.txt";

            using FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            Stream workingStream = useSynchronized ? Stream.Synchronized(stream) : stream;

            // Create multiple tasks that write to the stream concurrently
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    byte[] data = Encoding.UTF8.GetBytes($"Thread {threadId}: Hello from concurrent thread\n");

                    try
                    {
                        // Seek to end and write
                        workingStream.Seek(0, SeekOrigin.End);
                        workingStream.Write(data, 0, data.Length);
                        workingStream.Flush();
                        Console.WriteLine($"Thread {threadId} wrote successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Thread {threadId} error: {ex.Message}");
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"File size: {new FileInfo(filename).Length} bytes");
        }

        /// <summary>
        /// File class provides a set of static methods for read/write files in
        /// Text, Line and Binary formats.
        /// For qick file operations, File class is more convenient than FileStream class.
        /// </summary>
        internal static void FileStreamCreation()
        {
            FileStream fileStreamCreate = System.IO.File.Create("test-file-in-create.txt");
            
            try
            {
                if (System.IO.File.Exists("test-file-in-open-read.txt"))
                    System.IO.File.Delete("test-file-in-open-read.txt");

                FileStream fileStreamOpenRead = System.IO.File.OpenRead("test-file-in-open-read.txt");

            }
            catch (FileNotFoundException ex)
            {

                Console.WriteLine($"File.OpenRead can not create file: {ex.Message}");
            }

            try
            {
                FileStream fileStreamOpenWrite = System.IO.File.OpenWrite("test-file-in-open-write.txt");
                string content = "Hello, World!";
                byte[] buffer = new byte[1024];
                MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                fileStreamOpenWrite.Write(memoryStream.ToArray(), 0, content.Length);
                fileStreamOpenWrite.Close();
                fileStreamOpenWrite = System.IO.File.OpenWrite("test-file-in-open-write.txt");

                content = " content is updated";
                fileStreamOpenWrite.Write(Encoding.UTF8.GetBytes(content), 0, content.Length);
                fileStreamOpenWrite.Close();

                fileStreamOpenWrite = System.IO.File.OpenWrite("test-file-in-open-write.txt");
                _ = fileStreamOpenWrite.Read(buffer, 0, content.Length); // File.OpenWrite can not read from the file, it will throw an exception because the file is opened for writing only.




            }
            catch (NotSupportedException ex)
            {
                // even if file exists openwrite should not throw exception, it will just open the file for writing and truncate it to 0 bytes. If the file does not exist, it will create a new file. So no exception will be thrown in this case.
                Console.WriteLine($"File.OpenWrite cannot read error: {ex.Message}");
            }




        }

        internal static void FileReadCanBeLazyLoading()
        {
            // 1. Truly LAZY - deferred execution, nothing happens yet
            var query = System.IO.File.ReadLines("file.txt").Where(l => l.Length > 80);
            // No file I/O yet!

            // 2. Your example - STREAMING but EAGER execution
            var count = System.IO.File.ReadLines("file.txt").Count(l => l.Length > 80);
            // Reads line-by-line (streaming) but processes ALL lines NOW (eager)

            // 3. NOT lazy at all - loads everything into memory
            var lines = System.IO.File.ReadAllLines("file.txt");
            var count2 = lines.Count(l => l.Length > 80);
            // All lines in memory at once
        }

        internal static void SpecialPaths()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Console.WriteLine($"Desktop directory: {desktopPath}");

            string appdomainBase = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"AppDomain Base Directory: {appdomainBase}");
            string tempPath = Path.GetTempPath();
            Console.WriteLine($"Temporary directory: {tempPath}");
            string tempFile = Path.GetTempFileName();
            Console.WriteLine($"Temporary file created: {tempFile}");
        }

        internal static void FileStaticExample()
        {
            string path = "myfile.txt";

            // Each call performs full security checks
            bool exists = System.IO.File.Exists(path);              // Security check #1
            long size = new System.IO.FileInfo(path).Length;        // Security check #2
            DateTime created = System.IO.File.GetCreationTime(path); // Security check #3
            DateTime modified = System.IO.File.GetLastWriteTime(path); // Security check #4

            // 4 separate security checks!

            FileInfo fileInfo = new FileInfo("myfile.txt");  // Security check ONCE

            // All subsequent operations use cached information
            bool existsInstance = fileInfo.Exists;           // No security check
            long sizeInstance = fileInfo.Length;             // No security check
            DateTime createdInstance = fileInfo.CreationTime; // No security check
            DateTime modifiedInstance = fileInfo.LastWriteTime; // No security check

            // Only 1 security check for all operations!
        }

        internal static void NamedPipeExample()
        {
            string pipeName = "MyTestPipe";

            // Server task - runs in background
            Task serverTask = Task.Run(() =>
            {
                using var server = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    maxNumberOfServerInstances: 1);

                Console.WriteLine("[SERVER] Named pipe server created, waiting for client connection...");
                server.WaitForConnection(); // Blocks until a client connects
                Console.WriteLine("[SERVER] Client connected!");

                // Read message from client
                StreamReader reader = new StreamReader(server);

                string messageFromClient = reader.ReadLine();
                Console.WriteLine($"[SERVER] Received from client: {messageFromClient}");

                // Send response to client
                StreamWriter writer = new StreamWriter(server);
                writer.WriteLine("Hello Client, message received!");
                writer.Flush();
                Console.WriteLine("[SERVER] Response sent to client");

                // Keep connection open briefly
                Thread.Sleep(100);
            });

            // Give server time to start
            Thread.Sleep(500);

            // Client - connects to the server
            // In real-world scenario, this would be in a separate process/application
            using (var client = new NamedPipeClientStream(
                serverName: ".",  // "." means local machine
                pipeName,
                PipeDirection.InOut))
            {
                Console.WriteLine("[CLIENT] Connecting to named pipe server...");
                client.Connect(timeout: 5000); // 5 second timeout
                Console.WriteLine("[CLIENT] Connected to server!");

                // Send message to server
                StreamWriter writer = new StreamWriter(client);
                writer.WriteLine("Hello Server, this is the client!");
                writer.Flush();
                Console.WriteLine("[CLIENT] Message sent to server");

                // Read response from server
                StreamReader reader = new StreamReader(client);
                string responseFromServer = reader.ReadLine();
                Console.WriteLine($"[CLIENT] Received from server: {responseFromServer}");
            }

            // Wait for server to finish
            serverTask.Wait();
            Console.WriteLine("[COMPLETE] Named pipe communication finished\n");
        }

        internal static void AnonymousPipeExample()
        {
            // Anonymous pipes are one-way only (simplex)
            // They're useful for parent-child process communication
            // In this example we'll simulate it with tasks

            using var serverPipe = new AnonymousPipeServerStream(
                PipeDirection.Out,
                HandleInheritability.Inheritable);

            Console.WriteLine("[PARENT] Anonymous pipe created");
            Console.WriteLine($"[PARENT] Pipe handle: {serverPipe.GetClientHandleAsString()}");

            // In real scenario, you'd pass this handle to a child process
            string clientHandle = serverPipe.GetClientHandleAsString();

            // Simulate child process with a task
            Task clientTask = Task.Run(() =>
            {
                using var clientPipe = new AnonymousPipeClientStream(
                    PipeDirection.In,
                    clientHandle);

                Console.WriteLine("[CHILD] Connected to parent's anonymous pipe");

                StreamReader reader = new StreamReader(clientPipe);

                // Read messages from parent
                string message;
                while ((message = reader.ReadLine()) != null)
                {
                    if (message == "QUIT")
                        break;

                    Console.WriteLine($"[CHILD] Received: {message}");
                }

                Console.WriteLine("[CHILD] Finished reading from pipe");
            });

            // Give child time to connect
            Thread.Sleep(200);

            // Parent sends messages to child
            using (StreamWriter writer = new StreamWriter(serverPipe))
            {
                writer.AutoFlush = true;

                Console.WriteLine("[PARENT] Sending messages to child...");
                writer.WriteLine("Message 1: Hello from parent");
                Thread.Sleep(100);

                writer.WriteLine("Message 2: Anonymous pipes are one-way");
                Thread.Sleep(100);

                writer.WriteLine("Message 3: Great for parent-child communication");
                Thread.Sleep(100);

                writer.WriteLine("QUIT"); // Signal to stop
                Console.WriteLine("[PARENT] All messages sent");
            }

            // Dispose the server pipe before client finishes reading
            serverPipe.DisposeLocalCopyOfClientHandle();

            clientTask.Wait();
            Console.WriteLine("[COMPLETE] Anonymous pipe communication finished\n");
        }

        internal static void NamedPipeMultipleClients()
        {
            string pipeName = "MultiClientPipe";
            int numberOfClients = 3;

            // Server that handles multiple clients
            Task serverTask = Task.Run(() =>
            {
                for (int i = 0; i < numberOfClients; i++)
                {
                    // Create a new pipe instance for each client
                    var server = new NamedPipeServerStream(
                        pipeName,
                        PipeDirection.InOut,
                        maxNumberOfServerInstances: numberOfClients);

                    int clientNumber = i + 1;

                    // Handle each client in a separate task
                    Task.Run(() =>
                    {
                        Console.WriteLine($"[SERVER] Instance {clientNumber} waiting for connection...");
                        server.WaitForConnection();
                        Console.WriteLine($"[SERVER] Client {clientNumber} connected!");

                        using (server)
                        {
                            StreamReader reader = new StreamReader(server);
                            StreamWriter writer = new StreamWriter(server) { AutoFlush = true };

                            string message = reader.ReadLine();
                            Console.WriteLine($"[SERVER] From client {clientNumber}: {message}");

                            writer.WriteLine($"Response to client {clientNumber}");
                        }
                    });
                }
            });

            Thread.Sleep(500); // Let server start

            // Multiple clients connecting simultaneously
            List<Task> clientTasks = new List<Task>();

            for (int i = 0; i < numberOfClients; i++)
            {
                int clientId = i + 1;
                clientTasks.Add(Task.Run(() =>
                {
                    using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
                    client.Connect(5000);

                    Console.WriteLine($"[CLIENT {clientId}] Connected!");

                    StreamWriter writer = new StreamWriter(client) { AutoFlush = true };
                    StreamReader reader = new StreamReader(client);

                    writer.WriteLine($"Hello from client {clientId}");
                    string response = reader.ReadLine();
                    Console.WriteLine($"[CLIENT {clientId}] Received: {response}");
                }));
            }

            Task.WaitAll(clientTasks.ToArray());
            serverTask.Wait(3000);
            Console.WriteLine("[COMPLETE] Multiple client example finished\n");
        }

        internal static void DecoratorStreams()
        {             // Example of using a decorator stream (BufferedStream) to add buffering to a FileStream
            using var fileStream = new FileStream("decorator_example.txt", FileMode.Create, FileAccess.Write);
            using var bufferedStream = new BufferedStream(fileStream);
            string content = "This is an example of using a BufferedStream as a decorator for a FileStream.";
            byte[] data = Encoding.UTF8.GetBytes(content);
            bufferedStream.Write(data, 0, data.Length);
            bufferedStream.Flush(); // Ensure all data is written to the underlying stream
            Console.WriteLine("Data written to file using BufferedStream decorator.");
        }

        internal static void DeflateStreamExample()
        {

            /*
             * Q. BufferedStream has read operation, but why we use StreamReader?
             // Each layer adds a specific capability:

                FileStream           // Layer 1: Read from disk (bytes)
                    ↓
                DeflateStream        // Layer 2: Decompress (bytes → bytes)
                    ↓
                BufferedStream       // Layer 3: Add buffering (bytes → bytes)
                    ↓
                StreamReader         // Layer 4: Convert to text (bytes → strings)
            
                // DeflateStream works with BYTES
                byte[] buffer = new byte[1024];
                int bytesRead = deflateStream.Read(buffer, 0, buffer.Length);
                // You get raw bytes - you need to manually convert to string

                // StreamReader works with TEXT/CHARACTERS
                string line = reader.ReadLine();  // ✅ Convenient!
                string allText = reader.ReadToEnd();  // ✅ Easy!

                using (MemoryStream input = new MemoryStream(compressedData))
                using (DeflateStream deflate = new DeflateStream(input, CompressionMode.Decompress))
                {
                    // You have to manually read bytes and convert
                    byte[] buffer = new byte[1024];
                    int bytesRead = deflate.Read(buffer, 0, buffer.Length);
    
                    // Manual encoding conversion
                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    
                    // What if text is larger than buffer? You need a loop!
                    // What about line breaks? You need to parse them yourself!
                    // What about multi-byte characters? You need to handle partial reads!
                }

             */

            /*
             TextReader (abstract)           TextWriter (abstract)
                ↓                               ↓
                ├─ StreamReader            ├─ StreamWriter      (Read/Write from STREAMS)
                ├─ StringReader            ├─ StringWriter      (Read/Write from STRINGS)
                └─ (your custom)           └─ (your custom)
             
            // 1. You can write methods that accept ANY text source
            internal static void ProcessText(TextReader reader)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line.ToUpper());
                }
            }

            // 2. Can be called with DIFFERENT implementations
            ProcessText(new StreamReader("file.txt"));        // From file
            ProcessText(new StringReader("test\ndata"));      // From string
            ProcessText(Console.In);                           // From console (also a TextReader!)
             
             */

            string originalText = "This is a test string that will be compressed using DeflateStream. " +
                                 "The more repetitive the data, the better the compression ratio!";

            // COMPRESSION
            byte[] compressedData;
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream deflate = new DeflateStream(output, CompressionMode.Compress))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(originalText);
                    deflate.Write(inputBytes, 0, inputBytes.Length);
                }
                compressedData = output.ToArray();
            }

            Console.WriteLine($"Original size: {originalText.Length} bytes");
            Console.WriteLine($"Compressed size: {compressedData.Length} bytes");
            Console.WriteLine($"Compression ratio: {(1 - (double)compressedData.Length / originalText.Length) * 100:F2}%");

            // DECOMPRESSION
            using (MemoryStream input = new MemoryStream(compressedData))
            using (DeflateStream deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(deflate))
            {

                string decompressedText = reader.ReadToEnd();
                Console.WriteLine($"Decompressed: {decompressedText}");
                Console.WriteLine($"Match: {originalText == decompressedText}");
            }
        }

        internal static void CompressFile()
        {
            string sourceFile = "large-file.txt";
            string compressedFile = "large-file.deflate";

            // Compress file
            using (FileStream sourceStream = System.IO.File.OpenRead(sourceFile))
            using (FileStream targetStream = System.IO.File.Create(compressedFile))
            using (DeflateStream deflate = new DeflateStream(targetStream, CompressionMode.Compress))
            {
                sourceStream.CopyTo(deflate);
            }

            FileInfo original = new FileInfo(sourceFile);
            FileInfo compressed = new FileInfo(compressedFile);

            Console.WriteLine($"Original: {original.Length:N0} bytes");
            Console.WriteLine($"Compressed: {compressed.Length:N0} bytes");
            Console.WriteLine($"Saved: {original.Length - compressed.Length:N0} bytes");
        }

        internal static void HttpResponseCompression()
        {
            // Simulating HTTP response compression
            string jsonResponse = "{\"users\":[{\"name\":\"John\",\"age\":30},{\"name\":\"Jane\",\"age\":25}]}";

            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream deflate = new DeflateStream(output, CompressionLevel.Optimal))
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonResponse);
                    deflate.Write(data, 0, data.Length);
                }

                byte[] compressed = output.ToArray();
                Console.WriteLine($"JSON size: {jsonResponse.Length} bytes");
                Console.WriteLine($"Compressed: {compressed.Length} bytes");
                Console.WriteLine($"Savings: {((1 - (double)compressed.Length / jsonResponse.Length) * 100):F1}%");

                // This is what gets sent over HTTP with Content-Encoding: deflate
            }
        }

       

        public static void Main() 
        {
            NetworkStreamExample();
        }

    }

}
