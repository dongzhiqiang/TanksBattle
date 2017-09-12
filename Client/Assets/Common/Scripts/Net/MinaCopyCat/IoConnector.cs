#region Header
/**
 * 名称：IoConnector
 
 * 日期：2015.11.3
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NetCore
{
    /**
     * Connects to endpoint, communicates with the server, and fires events to
     * {@link IoHandler}s.
     * <p>
     * Please refer to
     * <a href="../../../../../xref-examples/org/apache/mina/examples/netcat/Main.html">NetCat</a>
     * example.
     * <p>
     * You should connect to the desired socket address to start communication,
     * and then events for incoming connections will be sent to the specified
     * default {@link IoHandler}.
     * <p>
     * Threads connect to endpoint start automatically when
     * {@link #connect(SocketAddress)} is invoked, and stop when all
     * connection attempts are finished.
     *
     * @author <a href="http://mina.apache.org">Apache MINA Project</a>
     */
    public interface IoConnector : IoService
    {
        /**
          * Connects to the specified remote address.
          *
          * @return the {@link ConnectFuture} instance which is completed when the
          *         connection attempt initiated by this call succeeds or fails.
          */
        void connect(string address, int port);
    }
}