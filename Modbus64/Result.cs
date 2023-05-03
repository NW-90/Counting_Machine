
using System;

namespace Modbus64
{
 

    public enum Result
    {
        ACKNOWLEDGE = 5,
        BYTECOUNT = 0x130,
        CONNECT_ERROR = 200,
        CONNECT_TIMEOUT = 0xc9,
        CRC = 0x12e,
        DEMO_TIMEOUT = 0x3e8,
        FUNCTION = 0x132,
        GATEWAY_DEVICE_FAILED = 11,
        GATEWAY_PATH_UNAVAILABLE = 10,
        ILLEGAL_DATA_ADDRESS = 2,
        ILLEGAL_DATA_VALUE = 3,
        ILLEGAL_FUNCTION = 1,
        ISCLOSED = 0x12d,
        MEMORY_PARITY_ERROR = 8,
        NEGATIVE_ACKNOWLEDGE = 7,
        QUANTITY = 0x131,
        READ = 0xcb,
        RESPONSE = 0x12f,
        RESPONSE_TIMEOUT = 300,
        SLAVE_DEVICE_BUSY = 6,
        SLAVE_DEVICE_FAILURE = 4,
        SUCCESS = 0,
        TRANSACTIONID = 0x133,
        WRITE = 0xca
    }
}

