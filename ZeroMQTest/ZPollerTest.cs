using System;
using System.Configuration;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using ZeroMQ;

namespace ZeroMQTest
{
  [TestFixture]
  public class ZPollerTest
  {
    private readonly Action _doNothing = () => { };

    [Test]
    public void InitializeAndDisposePollerTest()
    {
      ZPoller poller = null;
      Assert.DoesNotThrow(()=> poller = new ZPoller());
      Assert.DoesNotThrow(() => poller.Dispose());
    }

    [Test]
    public void AddPollerTest()
    {
      ZPoller poller = new ZPoller();
      ZSocket socket = new ZSocket(ZSocketType.DEALER);
      Assert.IsTrue(poller.Add(socket, _doNothing, 1));
      Assert.DoesNotThrow(() => poller.Dispose());
    }

    [Test]
    public void EmptyPollTest()
    {
      ZPoller poller = new ZPoller();
      ZSocket socket = new ZSocket(ZSocketType.DEALER);
      Assert.IsTrue(poller.Add(socket, _doNothing, 1));
      Assert.DoesNotThrow(()=>poller.Poll());
      Assert.DoesNotThrow(() => poller.Dispose());
    }

    [Test]
    public void SimplePollTest()
    {
      ZSocketTest.DoWithConnectedSocketPair((sendSocket, receiveSocket) =>
      {
        ZPoller poller = new ZPoller();
        poller.Add(receiveSocket, ReceiveMessageHandler, 1);
        _receivedMessage = false;
        var pollTask = Task.Run(()=> poller.Poll());
        sendSocket.Send(ZSocketTest.CreateSingleFrameTestMessage());
        pollTask.Wait();
        Assert.IsTrue(_receivedMessage);
      });
    }

    private bool _receivedMessage;
    private void ReceiveMessageHandler()
    {
      _receivedMessage = true;
    }
  }
}