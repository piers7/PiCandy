#!/bin/sh
### BEGIN INIT INFO
# Provides:          picandy.sh
# Required-Start:    $local_fs $network $named $time $syslog
# Required-Stop:     $local_fs $network $named $time $syslog
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Description:       Script to run PiCandy in background
### END INIT INFO

# Based on template by Ivan Derevianko aka druss <drussilla7@gmail.com>
# See http://druss.co/2015/06/run-kestrel-in-the-background/

# To use:
# sudo cp picandy.sh /etc/init.d/
# sudo update-rc.d picandy.sh defaults

APP_NAME=picandy
APP_DIR=/home/pi/dev/PiCandy.ServerHost-bin
APP_EXE=PiCandy.ServerHost.exe
APP_PATH=$APP_DIR/$APP_EXE
APP_USR=

PIDFILE=$APP_DIR/$APP_NAME.pid
LOGFILE=$APP_DIR/$APP_NAME.log

# fix issue with DNX exception in case of two env vars with the same name but different case
TMP_SAVE_runlevel_VAR=$runlevel
unset runlevel

start() {
  if [ -f $PIDFILE ] && kill -0 $(cat $PIDFILE); then
    echo "$APP_NAME already running" >&2
    return 1
  fi
  echo "Starting $APP_NAME..." >&2
  su -c "start-stop-daemon -SbmCv -x /usr/bin/nohup -p \"$PIDFILE\" -- \"$APP_PATH\" > \"$LOGFILE\"" $APP_USR
  echo "$APP_NAME started" >&2
}

stop() {
  if [ ! -f "$PIDFILE" ] || ! kill -0 $(cat "$PIDFILE"); then
    echo "$APP_NAME not running" >&2
    return 1
  fi
  echo "Stopping $APP_NAME..." >&2
  start-stop-daemon -K -p "$PIDFILE"
  rm -f "$PIDFILE"
  echo "$APP_NAME started" >&2
}

case "$1" in
  start)
    start
    ;;
  stop)
    stop
    ;;
  restart)
    stop
    start
    ;;
  *)
    echo "Usage: $0 {start|stop|restart}"
esac

export runlevel=$TMP_SAVE_runlevel_VAR
