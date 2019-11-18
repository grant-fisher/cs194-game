import os
from PIL import Image
import sys

def extractFrames(inGif, outFolder):
    frame = Image.open(inGif)
    nframes = 0
    while frame:
        frame.save( '%s/%s-%s.png' % (outFolder, os.path.basename(inGif)[:-4], nframes ) , 'PNG')
        nframes += 1
        try:
            frame.seek( nframes )
        except EOFError:
            break;
    return True

inputfilename = sys.argv[1]
outputfolder = sys.argv[2]

extractFrames(inputfilename, outputfolder)

print('Read in GIF from:', inputfilename)
print('Saved to:', outputfolder)
