# Articulate_Extension
This extension provides forwarding of messages received on a named pipe to the currently active ArmA instance.

## Requirements
This extension requires a version of ArmA with support for external extensions, this was implemented in ArmA 2 OA v1.61, and all variants of ArmA and Take on Helicopters released since have support for this functionality.
This extension also requires a SQF script which calls the connect and read functions provided by this module in order to receive data from it.

```
error = "Articulate" callExtension "connect";

data = "Articulate" callExtension "read";
```

## Error Codes

- **404 Named Pipe Not Available**
  The extension was unable to connect to the expected named pipe, this likely indicates that the named pipe server is not running or that there is a permission issue.

- **409 Named Pipe Already Connected**
  Indicates that a connection has already been made to the named pipe previously, this can be ignored or treated as an error depending on the implementation.

- **204 No Data Available**
  Indicates that the extension has not received any new messages from the server since the last read request.

## Named Pipe Data Format
In order to allow data to be sent to the requesting script in message format, without them being sent partial data, this extension requires all data sent over the named pipe to have messages terminated with a 0x01 byte.
This byte will not be sent to the client script, however it should always be sent by the server to indicate the end of a message - failure to do so will result in the extension believing that a message continues indefinitely.

```csharp
namedSocket.Write("Some data\x1");
```