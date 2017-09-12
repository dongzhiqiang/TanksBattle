package com.engine.common.utils.io;

import java.io.File;
import java.io.FileFilter;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.Reader;
import java.io.Writer;
import java.nio.channels.FileChannel;
import java.util.Collections;
import java.util.Date;
import java.util.Stack;

public class FileUtils {

	public static void clearDirectory(File targetDirectory) {
		final Stack<File> directories = new Stack<File>();
		final Stack<File> files = new Stack<File>();
		final FileFilter directoryFilter = new FileFilter() {
			@Override
			public boolean accept(File file) {
				if (file.isDirectory()) {
					return true;
				}
				files.push(file);
				return false;
			}
		};
		directories.push(targetDirectory);
		while (!directories.isEmpty()) {
			final File directory = directories.pop();
			files.push(directory);
			Collections.addAll(directories, directory.listFiles(directoryFilter));
		}
		for(File file:files) {
			file.delete();
		}
	}
	
	@SuppressWarnings("resource")
	public static long copyFile(int cacheSize, File from, File to) throws IOException {
		final long time = new Date().getTime();
		
		final FileInputStream in = new FileInputStream(from);
		final FileOutputStream out = new FileOutputStream(to);
		final FileChannel inChannel = in.getChannel();
		final FileChannel outChannel = out.getChannel();

		int length;
		while (true) {
			if (inChannel.position() == inChannel.size()) {
				inChannel.close();
				outChannel.close();
				return new Date().getTime() - time;
			}
			if ((inChannel.size() - inChannel.position()) < cacheSize) {
				length = (int) (inChannel.size() - inChannel.position());
			} else {
				length = cacheSize;
			}
			inChannel.transferTo(inChannel.position(), length, outChannel);
			inChannel.position(inChannel.position() + length);
		}
	}
	
	
	public static void closeReader(Reader inputStream) {
		if (inputStream != null) {
			try {
				inputStream.close();
			} catch (Exception exception) {
				exception.printStackTrace();
			}
		}
	}

	public static void closeWriter(Writer outputStream) {
		if (outputStream != null) {
			try {
				outputStream.close();
			} catch (Exception exception) {
				exception.printStackTrace();
			}
		}
	}

	public static void closeInputStream(InputStream inputStream) {
		if (inputStream != null) {
			try {
				inputStream.close();
			} catch (Exception exception) {
				exception.printStackTrace();
			}
		}
	}

	public static void closeOutputStream(OutputStream outputStream) {
		if (outputStream != null) {
			try {
				outputStream.close();
			} catch (Exception exception) {
				exception.printStackTrace();
			}
		}
	}
	
}
