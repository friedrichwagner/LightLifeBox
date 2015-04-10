#!/bin/bash
# /etc/init.d/llbox.sh

### BEGIN INIT INFO
# Provides:          llbox.sh
# Required-Start:    $remote_fs $syslog
# Required-Stop:     $remote_fs $syslog
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Short-Description: Startup LightLifeBox at raspi boot
# Description:       start / stop a program at boot 
### END INIT INFO

# If you want a command to always run, put it here

# Carry out specific functions when asked to by the system
# Carry out specific functions when asked to by the system
case "$1" in
  start)
    echo "Starting llbox"
    # run application you want to start
    /lightlife/run/llbox -b "ControlBox3" > /dev/null 2> /dev/null &
    ;;
  stop)
    echo "Stopping llbox"
    # kill application you want to stop
    killall llbox
    ;;
  *)
    echo "Usage: /etc/init.d/llbox.sh {start|stop}"
    exit 1
    ;;
esac

 exit 0 
 


