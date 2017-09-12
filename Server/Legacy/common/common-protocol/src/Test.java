import java.nio.Buffer;
import java.nio.ByteBuffer;
import java.util.List;

import com.engine.common.codec.Types;


public class Test {
	public static void main(String[] args) {
		ByteBuffer buf = null;
		// 中文
		new Test().read(buf); 
		
	}

    protected List<?> objectTable;
    protected List<?> stringTable;
    
	
	private Object read(ByteBuffer buf) {
		byte  b = buf.get();
		if(Types.NULL == b) {
			return null;
		} else if(Types.ZERO == b) {
			return 0;
		} else if(Types.FALSE == b) {
			return false;
		} else if(Types.TRUE == b) {
			return true;
		} else if(Types.FLOAT == b) {
			return buf.getFloat();
		} else if(Types.DOUBLE == b) {
			return buf.getDouble();			
		} else if(Types.RAW_OBJECT == b) {
			// TODO
		} else if(Types.OBJECT_POOLS == b) {	
			// TODO
		} else if(Types.ARRAY == (b & Types.ARRAY)) {
			int sub =  b & Types.ARRAY_MASK;
			int len = getInteger(buf, sub); 
			if(Types.BYTE_ARRAY == (b & Types.ARRAY)) {
				byte[] dst = new byte[len];
				buf.get(dst);
				return dst;
			}
			// 
			Object[] arr = new Object[len];
			for(int i = 0; i < len; i++) {
				Object o = read(buf);
				arr[i] = o;
			}
			return arr;
		} else if(Types.INTEGER == (b & Types.INTEGER)) {
			int sub =  b & Types.ARRAY_MASK;
			if(Types.INTEGER_0 == (b & Types.INTEGER)) {
				return sub;
			}
			int len = getInteger(buf, sub);
			if(len > 4) { // LONG
				if(len == 5) {
					return buf.getInt() << 32 + buf.get();
				} else if(len == 6) {
					return buf.getInt() << 32 + buf.getShort();
				} else if(len == 7) {
					return buf.getInt() << 32 + buf.getShort() << 16 + buf.get();
				} else if(len == 8) {
					return buf.getLong();
				} 
			} 
			return getInteger(buf, len);
		} else if(Types.STRING == (b & Types.STRING)) {
			int sub =  b & Types.ARRAY_MASK;
			if(Types.STRING_0 == (b & Types.STRING)) {
				byte[] dst = new byte[sub];
				buf.get(dst);
				return String.valueOf(dst);
			}
			int len = getInteger(buf, sub);
			byte[] dst = new byte[len];
			return String.valueOf(dst);
		} else if(Types.OBJECT_REF == (b & Types.OBJECT_REF)) {
			// TODO
		}
		
	}

	private int getInteger(ByteBuffer buf, int len) {
		int value = 0;
		if(len == 0) {
			return 0;
		} else if(len == 1) {
			value = buf.get();
		} else if(len == 2) {
			value = buf.getShort();
		} else if(len == 3) {
			value = buf.getShort() << 8 + buf.get();
		} else if(len == 4) {
			value = buf.getInt();
		}
		return value;
	}

}
