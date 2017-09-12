package com.engine.common.event.sample;

import org.springframework.stereotype.Component;

import com.engine.common.event.AbstractReceiver;

@Component
public class TestOneEventReceiver extends AbstractReceiver<TestEvent>{

	@Override
	public String[] getEventNames() {
		return new String[]{TestOneEvent.NAME,TestTwoEvent.NAME};
	}

	@Override
	public void doEvent(TestEvent event) {
		event.logicTest();
	}
}
