package com.engine.common.utils.reflect;

import java.beans.BeanInfo;
import java.beans.IntrospectionException;
import java.beans.Introspector;
import java.beans.PropertyDescriptor;
import java.lang.annotation.Annotation;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;
import java.util.WeakHashMap;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.util.ReflectionUtils;

/**
 * 反射工具类
 * @see {@link ReflectionUtils}
 * 
 */
public abstract class ReflectionUtility extends ReflectionUtils {
	private static final Logger log = LoggerFactory.getLogger(ReflectionUtility.class);
	private static final WeakHashMap<Class<?>, PropertyDescriptor[]> descriptorsCache = new WeakHashMap<Class<?>, PropertyDescriptor[]>();

	/**
	 * 查找唯一被指定注释声明的域
	 * @param <A> 注释类型
	 * @param clz 被查找的类
	 * @param type 指定的注释
	 * @return 不存在会返回 null
	 */
	public static <A extends Annotation> Field findUniqueFieldWithAnnotation(Class<?> clz, final Class<A> type) {
		final List<Field> fields = new ArrayList<Field>();
		doWithFields(clz, new FieldCallback() {
			@Override
			public void doWith(Field field) throws IllegalArgumentException, IllegalAccessException {
				fields.add(field);
			}
		}, new FieldFilter() {
			@Override
			public boolean matches(Field field) {
				return field.isAnnotationPresent(type);
			}
		});

		if (fields.size() > 1) {
			throw new IllegalStateException("被注释" + type.getSimpleName() + "声明的域不唯一");
		} else if (fields.size() == 1) {
			return fields.get(0);
		}
		return null;
	}

	/**
	 * 类似{@link org.springframework.util.ReflectionUtils#doWithFields(Class, FieldCallback, FieldFilter)}
	 * 的方法，只是该方法不会递归检查父类上的域
	 * @see org.springframework.util.ReflectionUtils#doWithFields(Class, FieldCallback, FieldFilter)
	 * @param clazz
	 * @param fc
	 * @param ff
	 * @throws IllegalArgumentException
	 */
	public static void doWithDeclaredFields(Class<?> clazz, FieldCallback fc, FieldFilter ff)
			throws IllegalArgumentException {
		if (clazz == null || clazz == Object.class) {
			return;
		}
		Field[] fields = clazz.getDeclaredFields();
		for (Field field : fields) {
			if (ff != null && !ff.matches(field)) {
				continue;
			}
			try {
				fc.doWith(field);
			} catch (IllegalAccessException ex) {
				throw new IllegalStateException("非法访问属性 '" + field.getName() + "': " + ex);
			}
		}
	}

	/**
	 * 获得第一个使用指定注释声明的属性
	 * @param clz 属性所在类
	 * @param annotationClass 注释类型
	 * @return 不存在则返回 null
	 */
	public static Field getFirstDeclaredFieldWith(Class<?> clz, Class<? extends Annotation> annotationClass) {
		for (Field field : clz.getDeclaredFields()) {
			if (field.isAnnotationPresent(annotationClass)) {
				return field;
			}
		}
		return null;
	}

	/**
	 * 获得全部使用指定注释声明的属性
	 * @param clz 属性所在类
	 * @param annotationClass 注释类型
	 * @return 不会返回 null
	 */
	public static Field[] getDeclaredFieldsWith(Class<?> clz, Class<? extends Annotation> annotationClass) {
		List<Field> fields = new ArrayList<Field>();
		for (Field field : clz.getDeclaredFields()) {
			if (field.isAnnotationPresent(annotationClass)) {
				fields.add(field);
			}
		}
		return fields.toArray(new Field[0]);
	}

	/**
	 * 获得第一个使用指定注释声明的方法
	 * @param clz 属性所在类
	 * @param annotationClass 注释类型
	 * @return 不存在则返回 null
	 */
	public static Method getFirstDeclaredMethodWith(Class<?> clz, Class<? extends Annotation> annotationClass) {
		for (Method method : clz.getDeclaredMethods()) {
			if (method.isAnnotationPresent(annotationClass)) {
				return method;
			}
		}
		return null;
	}

	/**
	 * 获得全部使用指定注释声明的方法
	 * @param clz 属性所在类
	 * @param annotationClass 注释类型
	 * @return 不会返回 null
	 */
	public static Method[] getDeclaredMethodsWith(Class<?> clz, Class<? extends Annotation> annotationClass) {
		List<Method> methods = new ArrayList<Method>();
		for (Method method : clz.getDeclaredMethods()) {
			if (method.isAnnotationPresent(annotationClass)) {
				methods.add(method);
			}
		}
		return methods.toArray(new Method[0]);
	}

	/**
	 * 获得全部使用指定注释声明的 get 方法
	 * @param clz 属性所在类
	 * @param annotationClass 注释类型
	 * @return 不会返回 null
	 */
	public static Method[] getDeclaredGetMethodsWith(Class<?> clz, Class<? extends Annotation> annotationClass) {
		List<Method> methods = new ArrayList<Method>();
		for (Method method : clz.getDeclaredMethods()) {
			if (method.getAnnotation(annotationClass) == null) {
				continue;
			}
			if (method.getReturnType().equals(void.class)) {
				continue;
			}
			if (method.getParameterTypes().length > 0) {
				continue;
			}
			methods.add(method);
		}
		return methods.toArray(new Method[0]);
	}

	/**
	 * 获取类属性PropertyDescriptor定义
	 * @param beanClass 类
	 * @return {@link PropertyDescriptor}[]
	 */
	public static PropertyDescriptor[] getPropertyDescriptors(Class<?> beanClass) {

		if (beanClass == null) {
			throw new IllegalArgumentException("No bean class specified");
		}

		// Look up any cached descriptors for this bean class
		PropertyDescriptor[] descriptors = null;
		descriptors = (PropertyDescriptor[]) descriptorsCache.get(beanClass);
		if (descriptors != null) {
			return (descriptors);
		}

		// Introspect the bean and cache the generated descriptors
		BeanInfo beanInfo = null;
		try {
			beanInfo = Introspector.getBeanInfo(beanClass);
		} catch (IntrospectionException e) {
			return (new PropertyDescriptor[0]);
		}
		descriptors = beanInfo.getPropertyDescriptors();
		if (descriptors == null) {
			descriptors = new PropertyDescriptor[0];
		}

		for (int i = 0; i < descriptors.length; i++) {
			PropertyDescriptor descriptor = descriptors[i];
			String name = descriptor.getName();
			// 忽略 getClass()
			if(name.equals("class")) {
				continue;
			}
			
			String propName = name.substring(0, 1).toUpperCase() + name.substring(1);

			// Getter
			Class<?> propertyType = descriptor.getPropertyType();
			if (descriptor.getReadMethod() == null) {
				String methodName = propertyType == boolean.class ? "is" : "get" + propName;
				Method readMethod = null;
				try {
					readMethod = beanClass.getDeclaredMethod(methodName);
				} catch (NoSuchMethodException e) {
					if (log.isDebugEnabled()) {
						log.debug("属性[{}]Getter方法[{}]不存在", name, methodName);
					}
				}
				if (readMethod != null) {
					try {
						descriptor.setReadMethod(readMethod);
					} catch (Exception e) {
						log.error("无法设置属性[{}]Getter[{}]方法", new Object[] { name, readMethod, e });
					}
				}
			}
			// Setter
			if (descriptor.getWriteMethod() == null) {
				String methodName = "set" + propName;
				Method writeMethod = null;
				try {
					writeMethod = beanClass.getDeclaredMethod(methodName, propertyType);
				} catch (NoSuchMethodException e1) {
					if (log.isDebugEnabled()) {
						log.debug("属性[{}]Setter方法[{}]不存在", name, methodName);
					}
				}
				if (writeMethod != null) {
					try {
						descriptor.setWriteMethod(writeMethod);
					} catch (Exception e) {
						log.error("无法设置属性[{}]Setter[{}]方法", new Object[] { name, writeMethod, e });
					}
				}
			}
		}
		// ----------------- Workaround for Bug 28358 ---------- END -------------------

		descriptorsCache.put(beanClass, descriptors);
		return (descriptors);

	}

}
