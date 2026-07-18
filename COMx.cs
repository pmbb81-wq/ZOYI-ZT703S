using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZOYI
{
    internal class COMx
    {
        SerialPort? port;
        String portName = "";
        int baudrate;
        volatile bool bConnected = false;
        private readonly object portLock = new object();

        /*
         * 
         */
        public COMx()
        {
            port = new SerialPort();
        }

        /*
         * 
         */
        public void connect(string com, int baud = 115200)
        {
            if (!bConnected)
            {
                lock (portLock)
                {
                    portName = com;
                    baudrate = baud;

                    try
                    {
                        port!.PortName = portName;
                        port.BaudRate = baudrate;
                        port.Parity = Parity.None;
                        port.DataBits = 8;
                        port.StopBits = StopBits.One;
                        port.ReadTimeout = 1000;

                        port.Open();

                        bConnected = true;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"COMx connect: {ex.Message}");
                    }
                }
            }
        }

        /*
         * 
         */
        public void disconnect()
        {
            lock (portLock)
            {
                if (port!.IsOpen)
                    port.Close();

                bConnected = false;
            }
        }

        /*
         * 
         */
        public int readByte()
        {
            int readByte = -1;

            if (bConnected)
            {
                try
                {
                    lock (portLock)
                    {
                        readByte = port!.ReadByte();
                    }
                }
                catch (TimeoutException) { throw new TimeoutException(); }
                catch (Exception ex) { throw new Exception($"COMx read: {ex.Message}"); }
            }

            return readByte;
        }

        /*
         * 
         */
        public bool CheckPortHealth()
        {
            if (!bConnected) return false;
            try
            {
                lock (portLock)
                {
                    if (port == null || !port.IsOpen)
                    {
                        bConnected = false;
                        return false;
                    }
                    _ = port.BytesToRead;
                    return true;
                }
            }
            catch
            {
                bConnected = false;
                return false;
            }
        }

        /*
         * 
         */
        public async Task<int> readByteAsync()
        {
            if (!bConnected)
                return -1;

            return await Task.Run(() =>
            {
                try
                {
                    lock (portLock)
                    {
                        return port!.ReadByte();
                    }
                }
                catch (TimeoutException)
                {
                    return -2;
                }
                catch
                {
                    bConnected = false;
                    return -1;
                }
            });
        }

        /*
         * 
         */
        public void write(string data)
        {
            if (bConnected)
            {
                lock (portLock)
                {
                    port!.Write(data);
                }
            }
        }

        public async Task writeAsync(string data)
        {
            if (bConnected)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        lock (portLock)
                        {
                            port!.Write(data);
                        }
                    }
                    catch { bConnected = false; }
                });
            }
        }

        /*
         * 
         */
        public string PortName()
        {
            return portName;
        }

        /*
         * 
         */
        public bool isConnected()
        {
            return bConnected;
        }

        /*
         * 
         */
        public List<string> listCOMports()
        {
            return SerialPort.GetPortNames().ToList();
        }
    }
}
