#include <node.h>
#include <node_buffer.h>
#include <algorithm>

using namespace v8;
using namespace node;

// 默认的密码
static unsigned int default_key[] = {
	0x3687C5E3,
	0xB7EF3327,
	0xE3791011,
	0x84E2D3BC
};

class Tea16
{	
private:
	static unsigned int _readUInt32(unsigned char data[], size_t offset)
	{
		return ((data[offset + 3] & 0xFF) << 0)
			+ ((data[offset + 2] & 0xFF) << 8)
			+ ((data[offset + 1] & 0xFF) << 16)
			+ ((data[offset + 0] & 0xFF) << 24);
	}

	static void _writeUInt32(unsigned int value, unsigned char data[], size_t offset)
	{
		data[offset + 3] = (unsigned char)(value >> 0);
		data[offset + 2] = (unsigned char)(value >> 8);
		data[offset + 1] = (unsigned char)(value >> 16);
		data[offset + 0] = (unsigned char)(value >> 24);
	}

	static void _encrypt(unsigned int v[], unsigned int k[])
	{
		unsigned int v0 = v[0], v1 = v[1], sum = 0, i;           /* set up */
		unsigned int delta = 0x9e3779b9;                     /* a key schedule constant */
		unsigned int k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];   /* cache key */
		for (i = 0; i < 16; i++)
		{
			/* basic cycle start */
			sum += delta;
			v0 += ((v1 << 4) + k0) ^ (v1 + sum) ^ ((v1 >> 5) + k1);
			v1 += ((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >> 5) + k3);
		} /* end cycle */

		v[0] = v0;
		v[1] = v1;
	}

	static void _decrypt(unsigned int v[], unsigned int k[])
	{
		unsigned int v0 = v[0], v1 = v[1], sum = 0xE3779B90, i;  /* set up */
		unsigned int delta = 0x9e3779b9;                     /* a key schedule constant */
		unsigned int k0 = k[0], k1 = k[1], k2 = k[2], k3 = k[3];   /* cache key */
		for (i = 0; i < 16; i++)
		{
			/* basic cycle start */
			v1 -= ((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >> 5) + k3);
			v0 -= ((v1 << 4) + k0) ^ (v1 + sum) ^ ((v1 >> 5) + k1);
			sum -= delta;
		} /* end cycle */

		v[0] = v0;
		v[1] = v1;
	}
    
    static void _processLeftBytes(unsigned char bytes[], size_t start, size_t len, unsigned int keys[])
	{
		for (unsigned int i = 0; i < len; ++i)
		{
			size_t index = start + i;
			unsigned char b = bytes[index];
			unsigned int k = (unsigned char)keys[i % 4];
			b = b ^ k;
			bytes[index] = b;
		}
	}

public:
	// 加密，如果length不是8的倍数，那么超过的字节不会加密
	// endpos所指不会被加密
	static void encrypt(unsigned char src[], size_t start, size_t length, unsigned int k[])
	{
		if (k == NULL)
			k = default_key;

		size_t offset = start;
		size_t count = length / 8;
		unsigned int v[] = { 0, 0 };		
		for (unsigned int i = 0; i < count; ++i)
		{
			v[0] = _readUInt32(src, offset);
			v[1] = _readUInt32(src, offset + 4);

			_encrypt(v, k);

			_writeUInt32(v[0], src, offset);
			_writeUInt32(v[1], src, offset + 4);

			offset += 8;
		}
        _processLeftBytes(src, start + count * 8, length % 8, k);
	}

	// 解密，如果length不是8的倍数，那么超过的字节不会解密
	// endpos所指不会被解密
	static void decrypt(unsigned char src[], size_t start, size_t length, unsigned int k[])
	{
		if (k == NULL)
			k = default_key;

		size_t offset = start;
		size_t count = length / 8;
		unsigned int v[] = { 0, 0 };		
		for (unsigned int i = 0; i < count; ++i)
		{
			v[0] = _readUInt32(src, offset);
			v[1] = _readUInt32(src, offset + 4);

			_decrypt(v, k);

			_writeUInt32(v[0], src, offset);
			_writeUInt32(v[1], src, offset + 4);

			offset += 8;
		}
        _processLeftBytes(src, start + count * 8, length % 8, k);
	}

};

void doEncryptOrDecrypt(bool doEncrypt, const FunctionCallbackInfo<Value>& args)
{
	Isolate* isolate = Isolate::GetCurrent();
	HandleScope scope(isolate);

	int argc = args.Length();
	if (argc < 1)
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 is missing")));
		return;
	}

	if (argc < 2)
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 2 is missing")));
		return;
	}

	Local<Value> arg1 = args[0];
	if (!Buffer::HasInstance(arg1))
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 must be Buffer object")));
		return;
	}

	Local<Value> arg2 = args[1];
	if (!arg2->IsArray())
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 2 must be Number array")));
		return;
	}

	Local<Array> arr = Local<Array>::Cast(arg2);
	unsigned int arrlen = arr->Length();
	if (arrlen < 4)
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 2 must contain at lease 4 elements")));
		return;
	}

	unsigned int keys[4];
	for (unsigned int i = 0; i < 4; ++i)
	{
		Local<Value> item = arr->Get(i);
		if (!item->IsNumber())
		{
			isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Element of argument 2 must be number")));
			return;
		}
		keys[i] = item->Uint32Value();
	}

	size_t len = Buffer::Length(arg1);
	char * buf = Buffer::Data(arg1);

	size_t start = 0;	//起始位置
	size_t oplen = len;	//处理长度

	if (argc >= 3)
	{
		//起始位置
		Local<Value> arg3 = args[2];
		if (arg3->IsNumber())
		{
			//保证>=0
			start = (size_t)std::max(0, arg3->Int32Value());
		}

		if (argc >= 4)
		{
			//长度
			Local<Value> arg4 = args[3];
			if (arg4->IsNumber())
			{
				//保证>=0
				oplen = (size_t)std::max(0, arg4->Int32Value());
			}
		}

		//根据len，校正start
		start = std::min(start, len);
		//根据start，校正oplen
		oplen = std::min(oplen, len - start);
	}
	if (doEncrypt)
		Tea16::encrypt((unsigned char *)buf, start, oplen, keys);
	else
		Tea16::decrypt((unsigned char *)buf, start, oplen, keys);
}

void encrypt(const FunctionCallbackInfo<Value>& args)
{
	doEncryptOrDecrypt(true, args);
}

void decrypt(const FunctionCallbackInfo<Value>& args)
{
	doEncryptOrDecrypt(false, args);
}

void MyInit(Handle<Object> exports) {
	NODE_SET_METHOD(exports, "encrypt", encrypt);
	NODE_SET_METHOD(exports, "decrypt", decrypt);
}

NODE_MODULE(tea16, MyInit)