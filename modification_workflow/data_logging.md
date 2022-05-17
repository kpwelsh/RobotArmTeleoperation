---
layout: default
title: Data Logging
nav_order: 3
parent: Modification Workflow
---


# Data Logging

Included in this project is abstract interface for secure database access (`SecureDatabase<T>`). This supports a few simple operations:

1. `Authorize` - Provide credentials to the database and determine if it acess is allowed.
2. `IsAuthorized` - Returns whether or not the current process is authorized.
3. `ReadRecord`, `WriteRecord`, `ListRecords` - Peforms basic database operations (requires authorization).

To use this for datalogging, you need to either make use of one of the existing implementations of `SecureDatabase`, or make your own. When provided with a serializable type `T`, the database will automatically serialize and deserialize the records for you. The following is an example of how to use this interface to log `TaskRecord` (sourced from `SerializationTest.cs`):


{% highlight csharp %}
...
taskDb = new RemoteDatabase<TaskRecord>(Url);
SystemManager.StartScene();
... // Some time later
SystemManager.EndScene();
// Attempt to authorize with some credentials
taskDb.Authorize("hcilab").ContinueWith(async (result) => {
    if (result.Result) {
        // If we are authorized, then write the record.
        await taskDb.WriteRecord(SystemManager.taskRecord, SystemManager.taskRecord.Name);
        // (Unnecessary) Read the record from the DB
        var taskRecord = await taskDb.ReadRecord(SystemManager.taskRecord.Name);
        // And replay it.
        SystemManager.ReplayScene(taskRecord);
    }
});
{% endhighlight %}

## `LocalDatabase`

The local implementation stores the data on disk using the C# OS API. If running natively, this does not requrie authorization. It only requires the path to the logging directory.

## `RemoteDatabase`

The remote implementation communicates with an HTTP server, which provides the necessary API implementation. To manage authentication, this class uses [JSON Web Tokens](https://jwt.io/), and relies on a secure HTTPS connection with the server to offer appropriate security. The server is required to support the following API endpoints:

* Authorize - `POST /login`

  REQUEST
```
{
    headers : {
        Accept : application/json,
        ContentType : application/json
    },
    body : {
        username : <credentials>
    }
}
```
RESPONSE
```
{
    ...
    body : {
        access_token : <JWT>
    }
}
```
* ReadRecord - `GET /record`

  REQUEST
```
{
    headers : {
        Accept : application/json,
        ContentType : application/json,
        Authorization : Bearer <JWT>
    },
    body : {
        name : <record identifier>
    }
}
```
RESPONSE
```
{
    ...
    body : <JSON Serialized Record>
}
```
* WriteRecord - `POST /record`


  REQUEST
```
{
    headers : {
        Accept : application/json,
        ContentType : application/json
        Authorization : Bearer <JWT>
    },
    body : {
        name : <record identifier>,
        data : <JSON Serialized Record>
    }
}
```
* ListRecords - `GET /listRecords`


  REQUEST
```
{
    headers : {
        Accept : application/json,
        ContentType : application/json
        Authorization : Bearer <JWT>
    }
}
```
RESPONSE
```
{
    ...
    body : {
        names : [<record identifier>, ...]
    }
}
```

An example of such a server can be found [here](https://github.com/kpwelsh/AuthenticatedServer-Template). (Warning, this example does not contain a transport layer security (TLS) protocol, and requires a secure server to be placed in front of it.)