# CMakeList.txt : CMake project for BMP280, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

project ("BMP280")

# Add source to this project's executable.
add_library (BMP280 SHARED "BMP280_driver/bmp280.c" "bmp280lib.c")
