# VizlabLSSPython
A visualization project created for Vizlab at Carnegie Observatories, showcasing the possibility to run a python script inside unity and updating a point cloud using the python output.
The desired project uses the gala python package, which calculates Milky Way stellar streams.
By adjusting input parameters of the stream with the UI sliders, the program runs a python script that calculates the final location of the stream and store the data in StreamingAssets.
The stream is then read in and instantiated as a point cloud (copying some codes from the Pcx package) with a random color.

This video shows the desired result, but in Vizlab gala can't be installed probably because of windows...
so I ended up creating a simulation of cosmic web formation using Zeldovich approximation.  That one also runs, but I don't like it...

[![Watch the video](https://img.youtube.com/vi/pmK4r1OxUHo/default.jpg)](https://youtu.be/pmK4r1OxUHo)
