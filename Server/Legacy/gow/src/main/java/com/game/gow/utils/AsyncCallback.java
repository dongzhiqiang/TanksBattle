package com.game.gow.utils;

public interface AsyncCallback<T> {
	
	void onSuccess(T result);

	void onError(Exception ex);
}
