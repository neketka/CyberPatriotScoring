import os
import subprocess


def check(args):
	try:
		os.setuid(os.geteuid())
		out = str(subprocess.check_output(args["command"].split(" ")))
		return args["query"] in out
	except:
		return False