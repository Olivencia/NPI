package teo.puntomovimientosonido;

import android.app.Activity;
import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorManager;
import android.media.MediaPlayer;
import android.os.Bundle;
import android.view.View;
import android.widget.ImageButton;

public class SwordActivity extends Activity {

    private ShakeDetectorTranq mShakeDetectorTranq;
    private ShakeDetectorFuerte mShakeDetectorFuerte;
    private SensorManager mSensorManager;
    private Sensor mAccelerometer;
    private MediaPlayer hit,lp,on,swing,off;
    private boolean state=false;
    ImageButton imgButton;


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //Button onoff = (Button) findViewById(R.id.onoff);
        setContentView(R.layout.activity_sword);
        //LinearLayout layout= new LinearLayout(this);
        hit = MediaPlayer.create(this,R.raw.hit);
        on= MediaPlayer.create(this,R.raw.on);
        off= MediaPlayer.create(this,R.raw.off);
        lp=MediaPlayer.create(this,R.raw.lp);
        swing=MediaPlayer.create(this,R.raw.swing);



        // ShakeDetectorTranq initialization
        mSensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        mAccelerometer = mSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);

        imgButton =(ImageButton)findViewById(R.id.onoff2);
        imgButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (state){
                    state = false;
                    off.start();
                    lp.pause();


                }
                else {
                    state = true;
                    on.start();
                    lp.setLooping(true);
                    lp.start();
                }

            }
        });

        mShakeDetectorFuerte = new ShakeDetectorFuerte(new ShakeDetectorFuerte.OnShakeListener() {

            @Override
            public void onShake() {
                if (state)
                hit.start();
            }
        });
        mShakeDetectorTranq = new ShakeDetectorTranq(new ShakeDetectorTranq.OnShakeListener() {

            @Override
            public void onShake() {
                if (state && !hit.isPlaying())
                swing.start();
            }

        });
    }

  /*  public void onoffClicked(View v) {
        if (state == true){
            state = false;
            lp.stop();

        }
        else {
            state = true;
            on.start();
            lp.start();
        }

    }*/


    @Override
    protected void onResume() {
        super.onResume();
        mSensorManager.registerListener(mShakeDetectorTranq, mAccelerometer, SensorManager.SENSOR_DELAY_UI);
        mSensorManager.registerListener(mShakeDetectorFuerte, mAccelerometer, SensorManager.SENSOR_DELAY_UI);
        if (state)
        lp.start();
    }


    @Override
    protected void onPause() {
        mSensorManager.unregisterListener(mShakeDetectorTranq);
        mSensorManager.unregisterListener(mShakeDetectorFuerte);
        super.onPause();
        if(state)
        lp.pause();
    }

}