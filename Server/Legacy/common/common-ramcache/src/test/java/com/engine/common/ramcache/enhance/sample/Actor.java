package com.engine.common.ramcache.enhance.sample;

import com.engine.common.ramcache.IEntity;

public class Actor implements IEntity{
  private Integer id;
  private String name;
  private IndexValue inva;
  public void setId(int id){
	  this.id=id;
  }
  
  public void setName(String name){
	  this.name=name;
  }
  
  public void setIndexValue(IndexValue inva){
	  this.inva=inva;
  }
  
  public Integer getId(){
	  return id;
  }
  
  public String getName(){
	  return name;
  }
  
  public IndexValue getIndexValue(){
	  return inva;
  }

}
