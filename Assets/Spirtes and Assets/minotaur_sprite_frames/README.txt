Sprite frame extraction

Each folder contains sequential transparent PNG frames for one animation.
The original image's frame-count labels do not match the visible frame count in three rows:
- idle: 3 visible frames (label says 4)
- walk: 9 visible frames (label says 8)
- death: 7 visible frames (label says 6)

The other rows match their labels: double_fist_ground_smash 8, charge 8, stomp 8, recovery 6.
The source sheet is a JPEG, so transparency is estimated by removing near-white background pixels.
