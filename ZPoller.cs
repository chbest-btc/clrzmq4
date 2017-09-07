using System;
using System.Runtime.InteropServices;
using ZeroMQ.lib;

namespace ZeroMQ
{
  public class ZPoller : IDisposable
  {
    private IntPtr _pollerPointer;
    private int _timeout = 1000;
    private int numberOfPoller = 0;

    public ZPoller()
    {
      _pollerPointer = zmq.poller_new();
    }

    public bool Add(ZSocket socket, Action userData, short events)
    {
      //ToDo: An Action<ZSocket> would be more useful. But this can not be used with Marshal.GetFunctionPointerForDelegate.
      IntPtr cb2 = Marshal.GetFunctionPointerForDelegate(userData);
      bool result = zmq.poller_add(_pollerPointer, socket.SocketPtr, cb2, events) != -1;
      if (result)
      {
        numberOfPoller++;
      }
      return result;
    }
    
    public void Poll()
    {
      Wait();
    }

    //implement analogue to the cppzmq implementation (https://github.com/zeromq/cppzmq/blob/master/zmq.hpp#L953) 
    private bool Wait()
    {
      zmq.ZmqPollerEvent[] events = new zmq.ZmqPollerEvent[numberOfPoller];
      int responceCode = zmq.poller_wait_all(_pollerPointer, events, numberOfPoller, _timeout);
      if (responceCode > 0)
      {
        for (int i = 0; i < numberOfPoller; i++)
        {
          if (events[i]._userData != null)
          {
            events[i]._userData.Invoke();
          }
        }
        return true;
      }
      var error = ZError.GetLastErr();
      if (Equals(error, ZError.ETIMEDOUT))
      {
        return false;
      }
      throw new Exception();
    }

    private bool Destroy()
    {
      var returnValue = zmq.poller_destroy(out _pollerPointer);
      _pollerPointer = IntPtr.Zero;
      return returnValue != -1;
    }

    public void Dispose()
    {
      Destroy();
    }
  }
}
