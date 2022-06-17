function BuildOpenCvForWindows($platform, $runMsbuild) {

    pushd .\External

    $buildDirectory = "opencv_build_win_${platform}"
    mkdir $buildDirectory -Force -ErrorAction Stop | Out-Null
    cd $buildDirectory
    pwd

    if ($platform -eq "x64") {
        $msbuildPlatform = "x64"
        $msmfFlag = "ON"
    }
    else {
        $msbuildPlatform = "Win32"
        $msmfFlag = "OFF" # opencv_videoio430.lib(cap_msmf.obj) : error LNK2001: unresolved external symbol _MFVideoFormat_H263 
    }

    cmake -G "Visual Studio 17 2022" `
        -A $msbuildPlatform `
        -D CMAKE_BUILD_TYPE=Release `
        -D CMAKE_INSTALL_PREFIX=install `
        -D INSTALL_C_EXAMPLES=OFF `
        -D INSTALL_PYTHON_EXAMPLES=OFF `
        -D BUILD_DOCS=OFF `
        -D BUILD_WITH_DEBUG_INFO=OFF `
        -D BUILD_DOCS=OFF `
        -D BUILD_EXAMPLES=OFF `
        -D BUILD_TESTS=OFF `
        -D BUILD_PERF_TESTS=OFF `
        -D BUILD_JAVA=OFF `
        -D BUILD_WITH_DEBUG_INFO=OFF `
        -D BUILD_opencv_apps=OFF `
        -D BUILD_opencv_calib3d=OFF `
        -D BUILD_opencv_datasets=OFF `
        -D BUILD_opencv_gapi=OFF `
        -D BUILD_opencv_java_bindings_generator=OFF `
        -D BUILD_opencv_js=OFF `
        -D BUILD_opencv_js_bindings_generator=OFF `
        -D BUILD_opencv_objc_bindings_generator=OFF `
        -D BUILD_opencv_python_bindings_generator=OFF `
        -D BUILD_opencv_python_tests=OFF `
        -D BUILD_opencv_ts=OFF `
        -D BUILD_opencv_world=OFF `
        -D BUILD_opencv_core=ON `
        -D BUILD_opencv_imgcodecs=ON `
        -D BUILD_opencv_imgproc=ON `
        -D BUILD_opencv_aruco=OFF `
        -D BUILD_opencv_calib3d=OFF `
        -D BUILD_opencv_apps=OFF `
        -D BUILD_opencv_aruco=OFF `
        -D BUILD_opencv_bgsegm=OFF `
        -D BUILD_opencv_bioinspired=OFF `
        -D BUILD_opencv_ccalib=OFF `
        -D BUILD_opencv_datasets=OFF `
        -D BUILD_opencv_dnn=OFF `
        -D BUILD_opencv_dnn_objdetect=OFF `
        -D BUILD_opencv_dpm=OFF `
        -D BUILD_opencv_face=OFF `
        -D BUILD_opencv_features2d=OFF `
        -D BUILD_opencv_flann=OFF `
        -D BUILD_opencv_fuzzy=OFF `
        -D BUILD_opencv_hfs=OFF `
        -D BUILD_opencv_highgui=ON `
        -D BUILD_opencv_img_hash=OFF `
        -D BUILD_opencv_java_bindings_gen=OFF `
        -D BUILD_opencv_js=OFF `
        -D BUILD_opencv_ts=OFF `
        -D BUILD_opencv_opencv_test_core=OFF `
        -D BUILD_opencv_line_descriptor=OFF `
        -D BUILD_opencv_ml=OFF `
        -D BUILD_opencv_objdetect=OFF `
        -D BUILD_opencv_optflow=OFF `
        -D BUILD_opencv_phase_unwrapping=OFF `
        -D BUILD_opencv_photo=OFF `
        -D BUILD_opencv_plot=OFF `
        -D BUILD_opencv_reg=OFF `
        -D BUILD_opencv_rgbd=OFF `
        -D BUILD_opencv_saliency=OFF `
        -D BUILD_opencv_shape=OFF `
        -D BUILD_opencv_stereo=OFF `
        -D BUILD_opencv_stitching=OFF `
        -D BUILD_opencv_structured_light=OFF `
        -D BUILD_opencv_superres=OFF `
        -D BUILD_opencv_surface_matching=OFF `
        -D BUILD_opencv_text=OFF `
        -D BUILD_opencv_tracking=OFF `
        -D BUILD_opencv_video=OFF `
        -D BUILD_opencv_videoio=OFF `
        -D BUILD_opencv_videostab=OFF `
        -D BUILD_opencv_world=OFF `
        -D BUILD_opencv_xfeatures2d=OFF `
        -D BUILD_opencv_ximgproc=OFF `
        -D BUILD_opencv_xobjdetect=OFF `
        -D BUILD_opencv_xphoto=OFF `
        -D WITH_CUDA=OFF `
        -D WITH_IPP=ON `
        -D WITH_FFMPEG=OFF `
        -D WITH_MSMF=${msmfFlag} `
        -D WITH_MSMF_DXVA=${msmfFlag} `
        -D WITH_QT=OFF `
        -D WITH_FREETYPE=OFF `
        -D WITH_TESSERACT=OFF `
        -D BUILD_SHARED_LIBS=OFF ../opencv 

    if ($runMsbuild) {
        # Developer Powershell for VS 2019 
        # Path: C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe -noe -c "&{Import-Module """C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"""; Enter-VsDevShell cebe9bd5}"
        # WorkDir: C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\

        msbuild INSTALL.vcxproj /t:build /p:configuration=Release /p:platform=$msbuildPlatform -maxcpucount
        ls
    }

    popd
}

# Entry point
If ((Resolve-Path -Path $MyInvocation.InvocationName).ProviderPath -eq $MyInvocation.MyCommand.Path) {

    ##### Change here #####
    $platform = "x64"
    #$platform = "x86"

    BuildOpenCvForWindows $platform $FALSE
}