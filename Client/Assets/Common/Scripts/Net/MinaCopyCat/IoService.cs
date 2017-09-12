#region Header
/**
 * 名称：IoService
 
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
     * Base interface for all {@link IoAcceptor}s and {@link IoConnector}s
     * that provide I/O service and manage {@link IoSession}s.
     *
     * @author <a href="http://mina.apache.org">Apache MINA Project</a>
     */
    public interface IoService
    {
        /**
         * Returns <tt>true</tt> if and if only all resources of this processor
         * have been disposed.
         */
        bool isDisposed();

        /**
         * Releases any resources allocated by this service.  Please note that
         * this method might block as long as there are any sessions managed by
         * this service.
         */
        void dispose();


        /**
         * Returns the handler which will handle all connections managed by this service.
         */
        IoHandler getHandler();

        /**
         * Sets the handler which will handle all connections managed by this service.
         */
        void setHandler(IoHandler handler);

        /**
         * Returns a value of whether or not this service is active
         *
         * @return whether of not the service is active.
         */
        bool isActive();
    }
}