VERSION = '0.2.0'
MSBUILD = r'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
XBUILD = 'xbuild'
RELEASE_CONFIG = '/p:Configuration=Release'
VM_TEMP_DIR = 'VmTemp/Source'

import shutil
import os
import io
import sys

def canonicalize_sep(path):
	return path.replace('/', os.sep).replace('\\', os.sep)
def canonicalize_newline(text, lineEnding):
	return text.replace("\r\n", "\n").replace("\r", "\n").replace("\n", lineEnding)

def copyDirectory(source, target, ext_filter = None):
	source = canonicalize_sep(source)
	target = canonicalize_sep(target)
	os.makedirs(target)
	for file in os.listdir(source):
		if ext_filter == None or file.endswith(ext_filter):
			fullpath = os.path.join(source, file)
			fulltargetpath = os.path.join(target, file)
			if os.path.isdir(fullpath):
				copyDirectory(fullpath, fulltargetpath)
			elif file.lower() in ('.ds_store', 'thumbs.db'):
				pass
			else:
				shutil.copyfile(fullpath, fulltargetpath)

def readFile(path):
	c = open(canonicalize_sep(path), 'rt')
	text = c.read()
	c.close()
	return text

def writeFile(path, content, lineEnding):
	content = canonicalize_newline(content, lineEnding)
	ucontent = unicode(content, 'utf-8')
	with io.open(canonicalize_sep(path), 'w', newline=lineEnding) as f:
		f.write(ucontent)

def runCommand(cmd):
	c = os.popen(cmd)
	output = c.read()
	c.close()
	return output

def main(args):
	librariesForRelease = [
		'Audio',
		'Core',
		'Easing',
		'FileIO',
		'FileIOCommon',
		'Game',
		'Gamepad',
		'Graphics2D',
		'GraphicsText',
		'Http',
		'ImageEncoder',
		'ImageResources',
		'ImageWebResources',
		'Json',
		'Math',
		'Random',
		'Resources',
		'UserData',
		'Web',
		'Xml',
	]

	if len(args) != 1:
		print("usage: python release.py windows|mono")
		return

	platform = args[0]

	if not platform in ('windows', 'mono'):
		print ("Invalid platform: " + platform)
		return

	copyToDir = 'crayon-' + VERSION + '-' + platform
	if os.path.exists(copyToDir):
		shutil.rmtree(copyToDir)
	os.makedirs(copyToDir)

	if platform == 'mono':
		print runCommand(' '.join([XBUILD, RELEASE_CONFIG, '../Compiler/CrayonOSX.sln']))
	else:
		print runCommand(' '.join([MSBUILD, RELEASE_CONFIG, r'..\Compiler\CrayonWindows.sln']))

	releaseDir = '../Compiler/bin/Release'
	shutil.copyfile(canonicalize_sep(releaseDir + '/Crayon.exe'), canonicalize_sep(copyToDir + '/crayon.exe'))
	shutil.copyfile(canonicalize_sep(releaseDir + '/LICENSE.txt'), canonicalize_sep(copyToDir + '/LICENSE.txt'))
	shutil.copyfile(canonicalize_sep('../README.md'), canonicalize_sep(copyToDir + '/README.md'))

	for file in filter(lambda x:x.endswith('.dll'), os.listdir(releaseDir)):
		shutil.copyfile(releaseDir + '/' + file, copyToDir + '/' + file)
	
	print runCommand(canonicalize_sep(copyToDir + '/crayon.exe') + ' -vm csharp-app -vmdir ' + canonicalize_sep(VM_TEMP_DIR))
	print runCommand(' '.join([MSBUILD, RELEASE_CONFIG, canonicalize_sep(VM_TEMP_DIR + '/CrayonRuntime.sln')]))
	
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/Vm', '.dll')
	copyDirectory(VM_TEMP_DIR + '/Libs/Release', copyToDir + '/Vm', '.exe')
	
	if platform == 'windows':
		setupFile = readFile("setup-windows.txt")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\r\n')
	if platform == 'mono':
		setupFile = readFile("setup-mono.md")
		writeFile(copyToDir + '/Setup Instructions.txt', setupFile, '\n')

	for lib in librariesForRelease:
		sourcePath = '../Libraries/' + lib
		targetPath = copyToDir + '/libs/' + lib
		copyDirectory(sourcePath, targetPath)

	print("Release directory created: " + copyToDir)

main(sys.argv[1:])
