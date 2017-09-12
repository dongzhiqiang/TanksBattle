package test;
import com.engine.common.protocol.annotation.Ignore;
import com.engine.common.protocol.annotation.Transable;

@Transable
public class Pet {
	private long id;
	private String name;
	private String ok = "EMPTY";
	private int a;
	private int bbbbbbbbbbbbbbbbb;
	private int cc;
	
	public static Pet valueOf(long id, String name) {
		Pet e = new Pet();
		e.id = id;
		e.name = name;
		return e;
	}

	public long getId() {
		return id;
	}

	public void setId(long id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	@Ignore
	public String getOk() {
		return ok;
	}
	
	public void setOk(String ok) {
		this.ok = ok;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + (int) (id ^ (id >>> 32));
		result = prime * result + ((name == null) ? 0 : name.hashCode());
		result = prime * result + ((ok == null) ? 0 : ok.hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		Pet other = (Pet) obj;
		if (id != other.id)
			return false;
		if (name == null) {
			if (other.name != null)
				return false;
		} else if (!name.equals(other.name))
			return false;
		if (ok == null) {
			if (other.ok != null)
				return false;
		} else if (!ok.equals(other.ok))
			return false;
		return true;
	}

	public int getA() {
		return a;
	}

	public void setA(int a) {
		this.a = a;
	}

	public int getBbbbbbbbbbbbbbbbb() {
		return bbbbbbbbbbbbbbbbb;
	}

	public void setBbbbbbbbbbbbbbbbb(int bbbbbbbbbbbbbbbbb) {
		this.bbbbbbbbbbbbbbbbb = bbbbbbbbbbbbbbbbb;
	}

	public int getCc() {
		return cc;
	}

	public void setCc(int cc) {
		this.cc = cc;
	}


	
}
