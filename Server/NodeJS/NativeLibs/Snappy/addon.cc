#include <node.h>
#include <node_buffer.h>
#include <algorithm>
#include <cstring>
#include "snappy/snappy.h"

using namespace std;
using namespace v8;
using namespace node;

void compress(const FunctionCallbackInfo<Value>& args)
{
	Isolate* isolate = Isolate::GetCurrent();
	HandleScope scope(isolate);

	int argc = args.Length();
	if (argc < 1)
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 is missing")));
		return;
	}

	Local<Value> arg1 = args[0];
	bool isBuffer = Buffer::HasInstance(arg1);
	bool isString = arg1->IsString();
	if (!isBuffer && !isString)
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 must be Buffer object or String")));
		return;
	}

	string dst;
	if (isBuffer)
	{
		size_t len = Buffer::Length(arg1);
		char * buf = Buffer::Data(arg1);
		snappy::Compress(buf, len, &dst);
	}
	else
	{
		String::Utf8Value utf8Str(arg1);
		size_t len = utf8Str.length();
		char * buf = *utf8Str;
		snappy::Compress(buf, len, &dst);
	}

	Local<Object> ret = Buffer::New(isolate, dst.length()).ToLocalChecked();
	memcpy(Buffer::Data(ret), dst.c_str(), dst.length());
	args.GetReturnValue().Set(ret);
}

void uncompress(const FunctionCallbackInfo<Value>& args)
{
	Isolate* isolate = Isolate::GetCurrent();
	HandleScope scope(isolate);

	int argc = args.Length();
	if (argc < 1)
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 is missing")));
		return;
	}

	Local<Value> arg1 = args[0];
	if (!Buffer::HasInstance(arg1))
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 must be Buffer object")));
		return;
	}

	bool asBuffer = false;
	if (argc >= 2)
	{
		Local<Value> arg2 = args[1];
		asBuffer = arg2->BooleanValue();
	}

	string dst;
	size_t len = Buffer::Length(arg1);
	char * buf = Buffer::Data(arg1);
	bool ok = snappy::Uncompress(buf, len, &dst);

	//如果解压失败，返回null
	if (!ok)
	{
		args.GetReturnValue().Set(Null(isolate));
	}
	else if (asBuffer)
	{
		Local<Object> ret = Buffer::New(isolate, dst.length()).ToLocalChecked();
		memcpy(Buffer::Data(ret), dst.c_str(), dst.length());
		args.GetReturnValue().Set(ret);
	}
	else
	{
		Local<String> ret = String::NewFromUtf8(isolate, (char *)dst.data(), NewStringType::kNormal, dst.length()).ToLocalChecked();
		args.GetReturnValue().Set(ret);
	}
}

void isValid(const FunctionCallbackInfo<Value>& args)
{
	Isolate* isolate = Isolate::GetCurrent();
	HandleScope scope(isolate);

	int argc = args.Length();
	if (argc < 1)
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 is missing")));
		return;
	}

	Local<Value> arg1 = args[0];
	if (!Buffer::HasInstance(arg1))
	{
		isolate->ThrowException(Exception::TypeError(String::NewFromUtf8(isolate, "Argument 1 must be Buffer object")));
		return;
	}

	size_t len = Buffer::Length(arg1);
	char * buf = Buffer::Data(arg1);
	bool ret = snappy::IsValidCompressedBuffer(buf, len);

	args.GetReturnValue().Set(ret);
}

void MyInit(Handle<Object> exports) {
	NODE_SET_METHOD(exports, "compress", compress);
	NODE_SET_METHOD(exports, "uncompress", uncompress);
	NODE_SET_METHOD(exports, "isValid", isValid);
}

NODE_MODULE(snappy, MyInit)