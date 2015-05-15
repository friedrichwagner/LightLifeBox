#include <functional>
#include <chrono>
#include <future>
#include <cstdio>

//
//http://stackoverflow.com/questions/14650885/how-to-create-timer-events-using-c-11
//
class Later
{
private:
	bool done=false;

public:
	template <class callable, class... arguments>
	Later(int after, bool async, callable&& f, arguments&&... args)
	{
		std::function<typename std::result_of<callable(arguments...)>::type()> task(std::bind(std::forward<callable>(f), std::forward<arguments>(args)...));
		if (async)
		{
			std::thread([after, task]() {
				std::this_thread::sleep_for(std::chrono::milliseconds(after));
					task();
			}).detach();
		}
		else
		{
			std::this_thread::sleep_for(std::chrono::milliseconds(after));
			if (!done)
				task();
		}
	}

	void Stop()
	{
		done = true;
	}

};

