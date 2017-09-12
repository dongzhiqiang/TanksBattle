using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class TeachAgent
{
    protected string m_teachName;

    public TeachAgent(string teachName)
    {
        m_teachName = teachName;
    }

    public string TeachName
    {
        get { return m_teachName; }
        set { m_teachName = value; }
    }

    public abstract TeachAgentType Type { get; }

    public abstract void Init(string param);

    public abstract void Release();
}
