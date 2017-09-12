package com.engine.common.event.sample;

import org.springframework.stereotype.Component;

import com.engine.common.event.AbstractReceiver;
@Component
public class TestEventReceiver extends AbstractReceiver<TestOneEvent>{

	@Override
	public String[] getEventNames() {
		return new String[]{TestOneEvent.NAME};
	}

	@Override
	public void doEvent(TestOneEvent event) {
		System.out.println("second receiver:"+event.getOperation());
		
	}

}
