//package com.engine.common.ramcache.orm.impl;
//
//import java.io.Serializable;
//import java.util.Collection;
//import java.util.Map;
//
//import org.springframework.beans.factory.annotation.Autowired;
//
//import com.engine.common.bdb.AccessorFactory;
//import com.engine.common.ramcache.IEntity;
//import com.engine.common.ramcache.orm.Accessor;
//import com.engine.common.ramcache.orm.EntityMetadata;
//
//@SuppressWarnings({"unchecked"})
//public class BdbAccessor implements Accessor {
//	
//	@Autowired
//	private AccessorFactory accessorFactory;
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> T load(Class<T> clz, PK id) {
//		com.engine.common.bdb.Accessor<PK, T> accessor = (com.engine.common.bdb.Accessor<PK, T>) accessorFactory.getAccessor(clz);
//		return accessor.get(id);
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> PK save(Class<T> clz, T entity) {
//		com.engine.common.bdb.Accessor<PK, T> accessor = (com.engine.common.bdb.Accessor<PK, T>) accessorFactory.getAccessor(clz);
//		return accessor.save(entity);
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> void remove(Class<T> clz, PK id) {
//		com.engine.common.bdb.Accessor<PK, T> accessor = (com.engine.common.bdb.Accessor<PK, T>) accessorFactory.getAccessor(clz);
//		accessor.delete(id);
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> T update(Class<T> clz, T entity) {
//		com.engine.common.bdb.Accessor<PK, T> accessor = (com.engine.common.bdb.Accessor<PK, T>) accessorFactory.getAccessor(clz);
//		accessor.update(entity);
//		return entity;
//	}
//
//	
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> void listAll(
//			Class<T> clazz, Collection<T> entities, Integer offset, Integer size) {
//		// TODO Auto-generated method stub
//	}
//	
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> void listIntersection(
//			Class<T> clz, Collection<T> entities, Map<String, Object> keyValue,
//			Integer offset, Integer size) {
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> void listUnion(
//			Class<T> clz, Collection<T> entities, Map<String, Object> keyValue,
//			Integer offset, Integer size) {
//	}
//
//	@Override
//	public Collection<EntityMetadata> getAllMetadata() {
//		// TODO Auto-generated method stub
//		return null;
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> void remove(
//			Class<T> clazz, T entity) {
//		com.engine.common.bdb.Accessor<PK, T> accessor = (com.engine.common.bdb.Accessor<PK, T>) accessorFactory.getAccessor(clazz);
//		accessor.delete(entity);
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> long countAll(
//			Class<T> clz) {
//		// TODO Auto-generated method stub
//		return 0;
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> long countIntersection(
//			Class<T> clz, Map<String, Object> keyValue) {
//		// TODO Auto-generated method stub
//		return 0;
//	}
//
//	@Override
//	public <PK extends Serializable, T extends IEntity<PK>> long countUnion(
//			Class<T> clz, Map<String, Object> keyValue) {
//		// TODO Auto-generated method stub
//		return 0;
//	}
//
//}
