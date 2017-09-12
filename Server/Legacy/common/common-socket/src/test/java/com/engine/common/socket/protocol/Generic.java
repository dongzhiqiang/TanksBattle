package com.engine.common.socket.protocol;

import com.engine.common.protocol.annotation.Transable;

@Transable
public class Generic<T1, T2> {

	private T1 one;
	private T2 two;
	
	public static <T1, T2> Generic<T1, T2> valueOf(T1 one, T2 two) {
		Generic<T1, T2> result = new Generic<T1, T2>();
		result.one = one;
		result.two = two;
		return result;
	}

	public T1 getOne() {
		return one;
	}

	public void setOne(T1 one) {
		this.one = one;
	}

	public T2 getTwo() {
		return two;
	}

	public void setTwo(T2 two) {
		this.two = two;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((one == null) ? 0 : one.hashCode());
		result = prime * result + ((two == null) ? 0 : two.hashCode());
		return result;
	}

	@SuppressWarnings("rawtypes")
	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		Generic other = (Generic) obj;
		if (one == null) {
			if (other.one != null)
				return false;
		} else if (!one.equals(other.one))
			return false;
		if (two == null) {
			if (other.two != null)
				return false;
		} else if (!two.equals(other.two))
			return false;
		return true;
	}

}
