# Interceptors

The TopNotify Daemon is made up of different interceptors.  
Each interceptor is supposed to handle a different notification type (eg. NativeInterceptor).  
An interceptor is comprised of a few main functions:  

- Start: Ran at daemon start
- Update: Ran 100 time per second, this is designed to position notifications
- Reflow: Ran every couple seconds, used to discover window handles and screen resolutions
- Restart: Called when settings changed from the GUI

# Dynamic Reflow

Dynamic reflow is a system that caches the window handles of the notifications, allowing for reducing CPU usage.
Instead of finding the window every interception update, it uses the cached one. 
When the cached handle eventually expires, the reflow system will find a new one and cache that.

# Standard & Fallback Discovery (Native Interceptor)

The caption of the notification window is different in other languages, so the NativeInterceptor needs a list of captions to find.
That's where the fallback discovery comes in. It can discover the notification window by calling NtUserBuildHwndList.
The problem with the fallback interceptor is reduced reliability and increased CPU usage. Dynamic reflow does help, but it's not as good as the standard discovery.
