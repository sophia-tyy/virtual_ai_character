# use command (uvicorn tts_server:app) to open server

from fastapi import FastAPI, Form
from fastapi.responses import StreamingResponse
import torch
import nltk
import styletts2.tts as tts
import io
import os

nltk.download('punkt_tab', quiet=True)

original_load = torch.load

def relaxed_load(*args, **kwargs):
    kwargs['weights_only'] = False
    return original_load(*args, **kwargs)

torch.load = relaxed_load

my_tts = tts.StyleTTS2(
    model_checkpoint_path='my_character_model\Models\LibriTTS\epoch_2nd_00019.pth',
    config_path='my_character_model\Models\LibriTTS\config.yml'
)

app = FastAPI()

@app.post("/tts")
def generate_tts(text: str = Form(...)):
    output_file = "temp_output.wav"
    
    my_tts.inference(
        text,
        target_voice_path="my_voice_sample.wav",
        output_wav_file=output_file
    )
    
    with open(output_file, "rb") as f:
        audio_bytes = f.read()
    
    os.remove(output_file)
    
    return StreamingResponse(io.BytesIO(audio_bytes), media_type="audio/wav")