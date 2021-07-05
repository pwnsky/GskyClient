CP := cp -r

none: pp.hh
	@echo "make install"

install:pp.hh
	@sudo mkdir -p /usr/include/pwnsky
	@sudo $(CP) $< /usr/include/pwnsky
	@echo 'c++ pp-sdk has installed'

uninstall:
	@sudo $(RM) /usr/include/pwnsky
	@echo ' c++ pp-sdk has uninstalled'
