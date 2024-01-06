from urllib.request import urlopen

trackers_file = open("trackers", "r")
trackers = trackers_file.readlines()
trackers_file.close()

repository = ""
for tracker in trackers:
    try:
        repository += urlopen(tracker.strip()).read().decode() + "\n"
    except:
        print("::error Couldn't fetch tracker " + tracker.strip())

repository = repository.strip() # get rid of last \n

print("Total tracks:", len(repository.split('\n')))
repository_file = open("repository", "w")
repository_file.write(repository)
repository_file.close()
